using System.Security.Cryptography;
using Microsoft.ML;

namespace AutoInsight.ML;

public interface IYardCapacityForecastService
{
    YardCapacityForecastResult Forecast(Guid yardId, int horizonHours, int capacity);
}

public sealed record YardCapacityForecastPoint(DateTime Timestamp, int ExpectedVehicles, float OccupancyRatio);

public sealed record YardCapacityForecastResult(Guid YardId, DateTime GeneratedAt, int Capacity, IReadOnlyList<YardCapacityForecastPoint> Points);

internal sealed class YardCapacityForecastService : IYardCapacityForecastService
{
    private const int MinimumHorizon = 1;
    private const int MaximumHorizon = 72;

    private readonly MLContext _mlContext;
    private readonly PredictionEngine<YardCapacityObservation, YardCapacityPrediction> _predictionEngine;

    public YardCapacityForecastService()
    {
        _mlContext = new MLContext(seed: 7321);

        var syntheticYardIds = GetSyntheticYardIds();
        var trainingData = syntheticYardIds.SelectMany(GenerateSyntheticObservations).ToList();

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms.Concatenate(
                "Features",
                nameof(YardCapacityObservation.HourSin),
                nameof(YardCapacityObservation.HourCos),
                nameof(YardCapacityObservation.DaySin),
                nameof(YardCapacityObservation.DayCos),
                nameof(YardCapacityObservation.YearSin),
                nameof(YardCapacityObservation.YearCos),
                nameof(YardCapacityObservation.BaselineLoad),
                nameof(YardCapacityObservation.PeakLoad),
                nameof(YardCapacityObservation.WeekendIndicator),
                nameof(YardCapacityObservation.WeekendAdjustment))
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.Regression.Trainers.FastTree(
                labelColumnName: nameof(YardCapacityObservation.OccupancyRatio),
                featureColumnName: "Features"));

        var model = pipeline.Fit(dataView);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<YardCapacityObservation, YardCapacityPrediction>(model);
    }

    public YardCapacityForecastResult Forecast(Guid yardId, int horizonHours, int capacity)
    {
        if (horizonHours < MinimumHorizon || horizonHours > MaximumHorizon)
        {
            throw new ArgumentOutOfRangeException(nameof(horizonHours), $"Horizon must be between {MinimumHorizon} and {MaximumHorizon} hours.");
        }

        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        }

        var config = CreateSimulationConfig(yardId) with { Capacity = capacity };
        var generatedAt = DateTime.UtcNow;
        var points = new List<YardCapacityForecastPoint>(horizonHours);

        for (var offset = 1; offset <= horizonHours; offset++)
        {
            var targetTimestamp = generatedAt.AddHours(offset);
            var input = CreateInputFromTimestamp(targetTimestamp, config);
            var prediction = _predictionEngine.Predict(input);
            var ratio = Math.Clamp(prediction.Score, 0f, 1f);
            var expectedVehicles = (int)Math.Round(ratio * config.Capacity);

            points.Add(new YardCapacityForecastPoint(targetTimestamp, expectedVehicles, ratio));
        }

    return new YardCapacityForecastResult(yardId, generatedAt, config.Capacity, points);
    }

    private static IEnumerable<YardCapacityObservation> GenerateSyntheticObservations(Guid yardId)
    {
        var config = CreateSimulationConfig(yardId);
        var random = CreateRandomForYard(yardId);

        var start = DateTime.UtcNow.Date.AddDays(-90);
        var totalHours = 24 * 90;

        for (var hour = 0; hour < totalHours; hour++)
        {
            var timestamp = start.AddHours(hour);
            var input = CreateInputFromTimestamp(timestamp, config);

            var weekendBoost = timestamp.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                ? config.WeekendAdjustment
                : 0f;

            var dailySeasonality = 0.5f + 0.5f * MathF.Sin(GetHourAngle(timestamp) - config.PeakPhase);
            var yearlySeasonality = 0.5f + 0.5f * MathF.Sin(GetYearAngle(timestamp));

            var occupancyRatio = config.BaseLoad
                                 + config.PeakLoad * dailySeasonality
                                 + yearlySeasonality * 0.05f
                                 + weekendBoost;

            var noise = (float)(random.NextDouble() * 2 - 1) * config.NoiseLevel;
            occupancyRatio = Math.Clamp(occupancyRatio + noise, 0.05f, 0.98f);

            yield return input with { OccupancyRatio = occupancyRatio };
        }
    }

    private static YardCapacityObservation CreateInputFromTimestamp(DateTime timestamp, YardSimulationConfig config)
    {
        var hourAngle = GetHourAngle(timestamp);
        var dayAngle = GetDayAngle(timestamp);
        var yearAngle = GetYearAngle(timestamp);
        var isWeekend = timestamp.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        return new YardCapacityObservation
        {
            HourSin = MathF.Sin(hourAngle),
            HourCos = MathF.Cos(hourAngle),
            DaySin = MathF.Sin(dayAngle),
            DayCos = MathF.Cos(dayAngle),
            YearSin = MathF.Sin(yearAngle),
            YearCos = MathF.Cos(yearAngle),
            BaselineLoad = config.BaseLoad,
            PeakLoad = config.PeakLoad,
            WeekendIndicator = isWeekend ? 1f : 0f,
            WeekendAdjustment = config.WeekendAdjustment,
            OccupancyRatio = 0f
        };
    }

    private static IEnumerable<Guid> GetSyntheticYardIds() =>
    [
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Guid.Parse("44444444-4444-4444-4444-444444444444"),
        Guid.Parse("55555555-5555-5555-5555-555555555555")
    ];

    private static YardSimulationConfig CreateSimulationConfig(Guid yardId)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        yardId.TryWriteBytes(guidBytes);
        var hash = SHA256.HashData(guidBytes);

        var baseLoad = 0.35f + (hash[0] / 255f) * 0.35f;
        var peakLoad = 0.15f + (hash[1] / 255f) * 0.25f;
        var weekendAdjustment = -0.12f + (hash[2] / 255f) * 0.24f;
        var noiseLevel = 0.02f + (hash[3] / 255f) * 0.06f;
        var capacity = 60 + hash[4] % 90;
        var peakPhase = (hash[5] / 255f) * (float)(2 * Math.PI);

        return new YardSimulationConfig(yardId, capacity, baseLoad, peakLoad, weekendAdjustment, noiseLevel, peakPhase);
    }

    private static Random CreateRandomForYard(Guid yardId)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        yardId.TryWriteBytes(guidBytes);
        var hash = SHA256.HashData(guidBytes);
        var seed = BitConverter.ToInt32(hash, 8);
        return new Random(seed);
    }

    private static float GetHourAngle(DateTime timestamp) => (float)(2 * Math.PI * timestamp.Hour / 24d);
    private static float GetDayAngle(DateTime timestamp) => (float)(2 * Math.PI * (int)timestamp.DayOfWeek / 7d);
    private static float GetYearAngle(DateTime timestamp) => (float)(2 * Math.PI * timestamp.DayOfYear / 365d);
}

internal sealed record YardSimulationConfig(
    Guid YardId,
    int Capacity,
    float BaseLoad,
    float PeakLoad,
    float WeekendAdjustment,
    float NoiseLevel,
    float PeakPhase);

internal sealed record YardCapacityObservation
{
    public float HourSin { get; init; }
    public float HourCos { get; init; }
    public float DaySin { get; init; }
    public float DayCos { get; init; }
    public float YearSin { get; init; }
    public float YearCos { get; init; }
    public float BaselineLoad { get; init; }
    public float PeakLoad { get; init; }
    public float WeekendIndicator { get; init; }
    public float WeekendAdjustment { get; init; }
    public float OccupancyRatio { get; init; }
}

internal sealed class YardCapacityPrediction
{
    public float Score { get; set; }
}

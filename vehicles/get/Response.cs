namespace AutoInsight.Vehicles.Get;

public record Response(
    Guid Id,
    string Plate,
    string Model,
    Guid OwnerId
);

namespace AutoInsight.Vehicles.Update;

public record Response(
    Guid Id,
    string Plate,
    string Model,
    Guid OwnerId
);

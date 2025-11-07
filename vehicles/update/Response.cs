namespace AutoInsight.Vehicles.Update;

public record Response(
    Guid Id,
    string Plate,
    string Model,
    string Status,
    DateTime EnteredAt,
    DateTime? LeftAt,
    Guid? AssigneeId
);

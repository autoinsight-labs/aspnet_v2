namespace AutoInsight.Vehicles.Get;

public record Response(
    Guid Id,
    string Plate,
    string Model,
    string Status,
    DateTime EnteredAt,
    DateTime? LeftAt,
    AssigneeResponse? Assignee
);

public record AssigneeResponse(Guid Id, string Name, string? ImageUrl, string Role, Guid UserId);

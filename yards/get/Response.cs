namespace AutoInsight.Yards.Get;

public record Response(
    Guid Id,
    string name,
    string OwnerId,
    ICollection<EmployeeResponse> Employees,
    ICollection<InviteResponse> Invites
);

public record EmployeeResponse(Guid Id, string Name, string? ImageUrl, string Role, string UserId);
public record InviteResponse(
    Guid Id,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    string InviterId
);

namespace AutoInsight.Yards.Get;

public record Response(
    Guid Id,
    string name,
    Guid OwnerId,
    ICollection<EmployeeResponse> Employees,
    ICollection<InviteResponse> Invites
);

public record EmployeeResponse(Guid Id, string Name, string? ImageUrl, string Role, Guid UserId);
public record InviteResponse(
    Guid Id,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    Guid InviterId
);

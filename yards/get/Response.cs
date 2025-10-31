using AutoInsight.Models;
namespace AutoInsight.Yards.Get;

public record Response(
    Guid Id,
    string name,
    Guid OwnerId,
    ICollection<EmployeeResponse> Employees,
    ICollection<EmployeeInvite> Invites
);

public record EmployeeResponse(Guid Id, string Name, string? ImageUrl, string Role, Guid UserId);

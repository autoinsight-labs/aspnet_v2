using AutoInsight.Models;
namespace AutoInsight.Yards.Get;

public record Response(
    Guid Id,
    string name,
    Guid OwnerId,
    ICollection<YardEmployee> Employees,
    ICollection<EmployeeInvite> Invites
);

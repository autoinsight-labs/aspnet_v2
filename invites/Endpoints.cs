using AutoInsight.EmployeeInvites.Create;
using AutoInsight.EmployeeInvites.Get;

namespace AutoInsight.EmployeeInvites
{
    public static class EmployeeInvitesEnpoints
    {
        public static RouteGroupBuilder MapEmployeeInviteEnpoints(this RouteGroupBuilder group)
        {
            var yardEmployeeInviteGroup = group.MapGroup("/yards/{yardId}/invites").WithTags("invite");
            var employeeInviteGroup = group.MapGroup("/invites").WithTags("invite");

            yardEmployeeInviteGroup.MapEmployeeInviteCreateEndpoint();
            employeeInviteGroup.MapEmployeeInviteGetEndpoint();

            return group;
        }
    }
}

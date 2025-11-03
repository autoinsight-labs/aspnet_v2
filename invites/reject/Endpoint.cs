using AutoInsight.Data;
using AutoInsight.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.Reject
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteRejectEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/{inviteId}/reject", HandleAsync);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string inviteId)
        {
            if (!Guid.TryParse(inviteId, out var parsedInviteId))
            {
                return Results.BadRequest(new { error = "InviteId must be a valid UUID." });
            }

            var invite = await db.EmployeeInvites
                        .FirstOrDefaultAsync(y => y.Id == parsedInviteId);
            if (invite is null)
                return Results.NotFound(new { error = "Invite not found" });

            if (invite.Status != InviteStatus.Pending)
                return Results.BadRequest(new { error = "Invite not available" });

            invite.Status = InviteStatus.Rejected;

            await db.SaveChangesAsync();

            return Results.Ok();
        }
    }
}

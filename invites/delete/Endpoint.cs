using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.Delete
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteDeleteEndpoint(this RouteGroupBuilder group)
        {
            group.MapDelete("/{inviteId}", HandleAsync);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string inviteId)
        {
            Guid parsedInviteId;
            if (string.IsNullOrEmpty(inviteId) || !Guid.TryParse(inviteId, out parsedInviteId))
                return Results.BadRequest(new { error = "'Invite Id' must be a valid UUID." });

            var invite = await db.EmployeeInvites.FirstOrDefaultAsync(v => v.Id == parsedInviteId);

            if (invite is null)
            {
                return Results.NotFound(new { error = "Invite not found" });
            }

            db.EmployeeInvites.Remove(invite);
            await db.SaveChangesAsync();

            return Results.Ok(new Response(invite.Id));
        }
    }
}

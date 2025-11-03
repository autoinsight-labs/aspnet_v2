using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.Get
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteGetEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/{inviteId}", HandleAsync);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string inviteId)
        {
            Guid parsedInviteId;
            if (string.IsNullOrEmpty(inviteId) || !Guid.TryParse(inviteId, out parsedInviteId))
                return Results.BadRequest(new { error = "'Invite Id' must be a valid UUID." });

            var invite = await db.EmployeeInvites
                            .Include(i => i.Yard)
                            .FirstOrDefaultAsync(y => y.Id == parsedInviteId);

            if (invite is null)
                return Results.NotFound(new { error = "Invite not found" });

            var response = new Response(invite.Id,
                        invite.Email,
                        invite.Role.ToString(),
                        invite.Status.ToString(),
                        invite.CreatedAt,
                        invite.AcceptedAt,
                        invite.InviterId,
                        new YardResponse(invite.Yard.Id, invite.Yard.Name)
              );
            return Results.Ok(response);
        }
    }
}

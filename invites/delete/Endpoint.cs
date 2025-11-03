using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.Delete
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteDeleteEndpoint(this RouteGroupBuilder group)
        {
            group.MapDelete("/{inviteId}", HandleAsync)
                .WithSummary("Delete an Employee Invite")
                .WithDescription(
                    "Deletes an existing Employee Invite identified by its unique ID. " +
                    "If the invite does not exist, returns 404 Not Found. If the provided ID is invalid, returns 400 Bad Request.\n\n" +
                    "### Example Request\n" +
                    "```\n" +
                    "DELETE /v2/invites/5a7c9d22-33e1-4b8b-9b51-23f8c4e4b6b2\n" +
                    "```\n\n" +
                    "### Example Response\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"id\": \"5a7c9d22-33e1-4b8b-9b51-23f8c4e4b6b2\"\n" +
                    "}\n" +
                    "```"
                )
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

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

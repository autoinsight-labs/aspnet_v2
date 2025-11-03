using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.Accept
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteAcceptEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/{inviteId}/accept", HandleAsync)
                .WithSummary("Accept an Employee Invite")
                .WithDescription(
                    "Accepts a pending Employee Invite identified by its UUID, creating a new `YardEmployee` linked to the corresponding Yard.\n\n" +
                    "**Route Parameter:**\n" +
                    "- `inviteId` (UUID, required): The unique identifier of the invite to accept.\n\n" +
                    "**Request Body:**\n" +
                    "- `name` (string, required): The name of the employee accepting the invite.\n" +
                    "- `imageUrl` (string, optional): URL to the employee’s profile picture.\n" +
                    "- `userId` (UUID, required): The unique identifier of the user accepting the invite.\n\n" +
                    "**Example Request:**\n" +
                    "```bash\n" +
                    "POST /v2/employee-invites/91af237a-59db-4c13-bf37-0cf6f3ec5a94/accept\n" +
                    "Content-Type: application/json\n" +
                    "\n" +
                    "{\n" +
                    "  \"name\": \"Arthur Mariano\",\n" +
                    "  \"imageUrl\": \"https://example.com/avatar.png\",\n" +
                    "  \"userId\": \"d5a90c87-fb15-4df7-86f3-982b6b8e53d1\"\n" +
                    "}\n" +
                    "```\n\n" +
                    "**Possible Responses:**\n" +
                    "- `200 OK`: Invite successfully accepted and employee created.\n" +
                    "- `400 Bad Request`: Invalid UUID or invite already accepted/declined.\n" +
                    "- `404 Not Found`: Invite not found.\n"
                )
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                When(x => x.ImageUrl is not null, () =>
                {
                    RuleFor(x => x.ImageUrl)
                        .NotEmpty()
                        .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                        .WithMessage("'ImageUrl' must be a valid URL.");
                });

                RuleFor(x => x.UserId).NotEmpty().Must(id => Guid.TryParse(id, out _)).WithMessage("'User Id' is not a valid UUID");
            }
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string inviteId)
        {
            if (!Guid.TryParse(inviteId, out var parsedInviteId))
            {
                return Results.BadRequest(new { error = "InviteId must be a valid UUID." });
            }

            var invite = await db.EmployeeInvites
                        .Include(i => i.Yard)
                        .FirstOrDefaultAsync(y => y.Id == parsedInviteId);
            if (invite is null)
                return Results.NotFound(new { error = "Invite not found" });

            if (invite.Status != InviteStatus.Pending)
                return Results.BadRequest(new { error = "Invite not available" });

            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            invite.AcceptedAt = DateTime.UtcNow;
            invite.Status = InviteStatus.Accepted;

            var employee = new YardEmployee
            {
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                Role = invite.Role,
                UserId = Guid.Parse(request.UserId),
                YardId = invite.YardId,
                Yard = invite.Yard
            };

            db.YardEmployees.Add(employee);

            await db.SaveChangesAsync();

            return Results.Ok();
        }
    }
}

using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.Create
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteCreateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/", HandleAsync)
                .WithSummary("Create a new Employee Invite")
                .WithDescription(
                    "Creates a new Employee Invite for a specific Yard, based on the provided email, role, and inviter ID. " +
                    "If the Yard does not exist, returns 404 Not Found. If the request is invalid, returns validation errors.\n\n" +
                    "### Example Request\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"email\": \"john.doe@example.com\",\n" +
                    "  \"role\": \"Member\",\n" +
                    "  \"inviterId\": \"550e8400-e29b-41d4-a716-446655440000\"\n" +
                    "}\n" +
                    "```\n\n" +
                    "### Example Response\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"id\": \"f27f2f3a-5d9b-4e1a-9f23-bc17f1e7e200\",\n" +
                    "  \"email\": \"john.doe@example.com\",\n" +
                    "  \"role\": \"Member\",\n" +
                    "  \"status\": \"Pending\",\n" +
                    "  \"createdAt\": \"2025-11-03T12:45:30Z\",\n" +
                    "  \"acceptedAt\": null,\n" +
                    "  \"inviterId\": \"550e8400-e29b-41d4-a716-446655440000\",\n" +
                    "  \"yard\": {\n" +
                    "    \"id\": \"9a4a2b66-2b29-4de7-82b2-8f3a3af88f66\",\n" +
                    "    \"name\": \"Central Yard\"\n" +
                    "  }\n" +
                    "}\n" +
                    "```"
                )
                .Produces<Response>(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Email).EmailAddress();
                RuleFor(x => x.Role).NotEmpty().Must(BeAValidRole)
                                .WithMessage("Role must be one of: Admin, Member"); ;
                RuleFor(x => x.InviterId).NotEmpty().Must(id => Guid.TryParse(id, out _)).WithMessage("'Inviter Id' is not a valid UUID");
            }

            private bool BeAValidRole(string role) =>
                            Enum.TryParse<EmployeeRole>(role, true, out _);
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string yardId)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
            {
                return Results.BadRequest(new { error = "YardId must be a valid UUID." });
            }

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsedYardId);
            if (yard is null)
                return Results.NotFound(new { error = "Yard not found" });

            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var invite = new EmployeeInvite
            {
                Email = request.Email,
                Role = Enum.Parse<EmployeeRole>(request.Role, true),
                InviterId = Guid.Parse(request.InviterId),
                YardId = parsedYardId,
                Yard = yard,
            };

            db.EmployeeInvites.Add(invite);

            await db.SaveChangesAsync();

            var response = new Response(invite.Id,
                        invite.Email,
                        invite.Role.ToString(),
                        invite.Status.ToString(),
                        invite.CreatedAt,
                        invite.AcceptedAt,
                        invite.InviterId,
                        new YardResponse(yard.Id, yard.Name)
                    );
            return Results.Created($"/v2/invites/{invite.Id}", response);
        }
    }
}

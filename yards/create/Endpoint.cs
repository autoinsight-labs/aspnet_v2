using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;

namespace AutoInsight.Yards.Create
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardCreateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/", HandleAsync)
                .WithName("CreateYard")
                .WithSummary("Create a new yard and owner admin")
                .WithDescription(
                    "Creates a new yard and automatically registers the provided owner as the first Admin employee for that yard." +
                    "\n\n**Request Body:**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"name\": \"Main Yard\",\n" +
                    "  \"ownerName\": \"Maria Souza\",\n" +
                    "  \"ownerUserId\": \"550e8400-e29b-41d4-a716-446655440000\"\n" +
                    "}\n" +
                    "```" +
                    "\n\n**Responses:**\n" +
                    "- `201 Created`: Yard created successfully (returns yard details).\n" +
                    "- `400 Bad Request`: Validation errors or invalid UUID." +
                    "\n\n**Example Response (201):**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"id\": \"7f5c1b8a-49df-4c4b-8b5f-bb56b0d1c8aa\",\n" +
                    "  \"name\": \"Main Yard\",\n" +
                    "  \"ownerId\": \"3d2e8f7b-12c4-4a83-a0af-9d7f86c0b8a1\"\n" +
                    "}\n" +
                    "```"
                )
                .Produces<Response>(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest);

            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.OwnerName).NotEmpty();
                RuleFor(x => x.OwnerUserId).NotEmpty().Must(id => Guid.TryParse(id, out _)).WithMessage("'Owner UserId' is not a valid UUID");
            }
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db)
        {
            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var ownerId = Guid.NewGuid();

            var yard = new Yard
            {
                Name = request.Name,
                OwnerId = ownerId
            };

            db.Yards.Add(yard);

            var employee = new YardEmployee
            {
                Id = ownerId,
                Name = request.OwnerName,
                Role = EmployeeRole.Admin,
                UserId = Guid.Parse(request.OwnerUserId),
                Yard = yard,
                YardId = yard.Id,
            };

            db.YardEmployees.Add(employee);

            await db.SaveChangesAsync();

            var response = new Response(yard.Id, yard.Name, yard.OwnerId);
            return Results.Created($"/v2/yards/{yard.Id}", response);
        }
    }
}

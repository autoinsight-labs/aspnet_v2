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
                            .WithSummary("Create a new Yard")
                                                        .WithDescription("Creates a new Yard entity with the provided name and owner ID. Returns 201 Created with the created Yard details, or validation errors if the request is invalid.\n" +
                                                            "### Example Request\n" +
                                                            "```json\n" +
                                                            "{\n" +
                                                            "  \"name\": \"Main Yard\",\n" +
                                                            "  \"ownerId\": \"550e8400-e29b-41d4-a716-446655440000\"\n" +
                                                            "}\n" +
                                                            "```\n\n" +
                                                            "### Example Response\n" +
                                                            "```json\n" +
                                                            "{\n" +
                                                            "  \"id\": \"7f5c1b8a-49df-4c4b-8b5f-bb56b0d1c8aa\",\n" +
                                                            "  \"name\": \"Main Yard\",\n" +
                                                            "  \"ownerId\": \"550e8400-e29b-41d4-a716-446655440000\"\n" +
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

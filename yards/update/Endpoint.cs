using AutoInsight.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Yards.Update
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardUpdateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPatch("/{yardId}", HandleAsync)
                .WithSummary("Update a Yard partially")
                .WithDescription(
                    "Updates one or more fields of an existing Yard identified by its ID.\n\n" +
                    "This endpoint supports partial updates — only the provided fields will be modified.\n\n" +
                    "**Path Parameter:**\n" +
                    "- `yardId` (UUID): The unique identifier of the Yard to update.\n\n" +
                    "**Request Body Example:**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"name\": \"Updated Yard Name\",\n" +
                    "  \"ownerId\": \"9f4f93c6-56e1-4f8f-a5e1-8b6b654a09dc\"\n" +
                    "}\n" +
                    "```\n\n" +
                    "**Possible Responses:**\n" +
                    "- `200 OK`: Returns the updated Yard.\n" +
                    "- `400 Bad Request`: Invalid Yard ID or payload.\n" +
                    "- `404 Not Found`: Yard does not exist.\n" +
                    "**Example Successful Response (200):**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"id\": \"9a3b2b1d-7e54-4b5a-93f3-5a4bfa351b1d\",\n" +
                    "  \"name\": \"Updated Yard Name\",\n" +
                    "  \"ownerId\": \"9f4f93c6-56e1-4f8f-a5e1-8b6b654a09dc\"\n" +
                    "}\n" +
                    "```"
                )
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .ProducesValidationProblem();
            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                When(x => x.Name is not null, () =>
                {
                    RuleFor(x => x.Name)
                                        .NotEmpty()
                                        .WithMessage("Name cannot be empty");
                });
                When(x => x.OwnerId is not null, () =>
                {
                    RuleFor(x => x.OwnerId)
                                        .NotEmpty()
                                        .Must(id => Guid.TryParse(id, out _))
                                        .WithMessage("'Owner Id' is not a valid UUID");
                });
            }
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string yardId)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
                return Results.BadRequest(new { error = "'Yard Id' must be a valid UUID." });

            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsedYardId);
            if (yard is null)
            {
                return Results.NotFound(new { error = "Yard not found" });
            }

            if (request.Name is not null) yard.Name = request.Name;
            if (request.OwnerId is not null) yard.OwnerId = Guid.Parse(request.OwnerId);

            await db.SaveChangesAsync();

            var response = new Response(yard.Id, yard.Name, yard.OwnerId);
            return Results.Ok(response);
        }
    }
}

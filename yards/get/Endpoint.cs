using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Yards.Get
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardGetEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/{yardId}", HandleAsync)
                            .WithSummary("Retrieve details of a Yard by ID")
                                            .WithDescription(
                                                            "Fetches detailed information about a Yard by its unique identifier.\n\n" +
                                                            "**Path Parameter:**\n" +
                                                            "- `yardId` (UUID): The unique identifier of the Yard.\n\n" +
                                                            "**Example Request:**\n" +
                                                            "```bash\n" +
                                                            "GET /v2/yards/6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\n" +
                                                            "```\n\n" +
                                                            "**Possible Responses:**\n" +
                                                            "- `200 OK`: Yard found and returned.\n" +
                                                            "- `400 Bad Request`: Invalid Yard ID format.\n" +
                                                            "- `404 Not Found`: Yard not found.\n\n" +
                                                            "**Example Response (200):**\n" +
                                                            "```json\n" +
                                                            "{\n" +
                                                            "  \"id\": \"6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\",\n" +
                                                            "  \"name\": \"Main Storage Yard\",\n" +
                                                            "  \"ownerId\": \"d5a90c87-fb15-4df7-86f3-982b6b8e53d1\",\n" +
                                                            // TODO: Add employee and invites examples when we get to it
                                                            "  \"employees\": [],\n" +
                                                            "  \"invites\": []\n" +
                                                            "}\n" +
                                                            "```"
                                            )
                                            .Produces<Response>(StatusCodes.Status200OK)
                                            .Produces(StatusCodes.Status400BadRequest)
                                            .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string yardId)
        {
            Guid parsed;
            if (string.IsNullOrEmpty(yardId) || !Guid.TryParse(yardId, out parsed))
                return Results.BadRequest(new { error = "'Yard Id' must be a valid UUID." });

            var yard = await db.Yards
                .Include(x => x.Employees)
                .Include(x => x.Invites)
                .FirstOrDefaultAsync(y => y.Id == parsed);

            if (yard is null)
                return Results.NotFound(new { error = "Yard not found" });

            var response = new Response(
                                yard.Id,
                                yard.Name,
                                yard.OwnerId,
                                yard.Employees.Select(e =>
                                    new EmployeeResponse(
                                                    e.Id,
                                                    e.Name,
                                                    e.ImageUrl,
                                                    e.Role.ToString(),
                                                    e.UserId)
                                    ).ToList(),
                                yard.Invites);
            return Results.Ok(response);
        }
    }
}

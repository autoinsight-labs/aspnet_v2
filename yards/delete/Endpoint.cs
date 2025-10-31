using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Yards.Delete
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardDeleteEndpoint(this RouteGroupBuilder group)
        {
            group.MapDelete("/{yardId}", HandleAsync)
                        .WithSummary("Delete a Yard by its ID")
                                                .WithDescription(
                                                        "Deletes a Yard from the system by its unique identifier.\n\n" +
                                                        "**Path Parameter:**\n" +
                                                        "- `yardId` (UUID): The unique identifier of the Yard.\n\n" +
                                                        "**Example Request:**\n" +
                                                        "```bash\n" +
                                                        "DELETE /v2/yards/6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\n" +
                                                        "```\n\n" +
                                                        "**Possible Responses:**\n" +
                                                        "- `200 OK`: Yard successfully deleted.\n" +
                                                        "- `400 Bad Request`: Invalid Yard ID format.\n" +
                                                        "- `404 Not Found`: Yard not found.\n\n" +
                                                        "**Example Response (200):**\n" +
                                                        "```json\n" +
                                                        "{ \"id\": \"6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\" }\n" +
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

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsed);

            if (yard is null)
            {
                return Results.NotFound(new { error = "Yard not found" });
            }

            db.Yards.Remove(yard);
            await db.SaveChangesAsync();

            return Results.Ok(new Response(yard.Id));
        }
    }
}

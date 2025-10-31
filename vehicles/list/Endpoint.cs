using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Vehicles.List
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapVehicleListEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/", HandleAsync)
            .WithSummary("List Vehicles with pagination support")
            .WithDescription(
                "Retrieves a paginated list of Vehicles ordered by their ID.\n\n" +
                "**Query Parameters:**\n" +
                "- `cursor` (UUID, optional): Used for pagination to fetch the next set of results.\n" +
                "- `limit` (integer, optional, default=10, max=100): Number of items to return.\n\n" +
                "**Example Request:**\n" +
                "```bash\n" +
                "GET /v2/vehicles?limit=5\n" +
                "```\n\n" +
                "**Pagination Example:**\n" +
                "```bash\n" +
                "GET /v2/vehicles?cursor=6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83&limit=5\n" +
                "```\n\n" +
                "**Possible Responses:**\n" +
                "- `200 OK`: Returns a paginated list of Vehicles.\n" +
                "- `400 Bad Request`: Invalid cursor or limit value.\n\n" +
                "**Example Response (200):**\n" +
                "```json\n" +
                "{\n" +
                "  \"items\": [\n" +
                "    { \"id\": \"6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\", \"plate\": \"ABC1234\", \"model\": \"MottuSport110i\", \"ownerId\": \"d5a90c87-fb15-4df7-86f3-982b6b8e53d1\" },\n" +
                "    { \"id\": \"7fbd32a2-1b78-4a2e-bf53-83f1c1fdd92b\", \"plate\": \"XYZ5678\", \"model\": \"HondaPop110i\", \"ownerId\": \"21e8c4e4-d38b-47cf-8022-f4bbf2d5f212\" }\n" +
                "  ],\n" +
                "  \"pageInfo\": {\n" +
                "    \"nextCursor\": \"9f1f3a93-bf6d-4028-91cb-238aaf3b2368\",\n" +
                "    \"hasNext\": true\n" +
                "  },\n" +
                "  \"count\": 2,\n" +
                "  \"meta\": {\n" +
                "    \"totalCount\": 152,\n" +
                "    \"availableModels\": [\"MottuSport110i\", \"Mottue\", \"HondaPop110i\", \"TVSSport110i\"]\n" +
                "  }\n" +
                "}\n" +
                "```"
            )
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string? cursor = null, int limit = 10)
        {
            if (limit <= 0 || limit > 100)
                return Results.BadRequest(new { error = "Limit must be between 1 and 100." });

            Guid? cursorGuid = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                if (!Guid.TryParse(cursor, out var parsed))
                    return Results.BadRequest(new { error = "Cursor must be a valid UUID." });

                bool exists = await db.Vehicles.AnyAsync(y => y.Id == parsed);
                if (!exists)
                    return Results.BadRequest(new { error = "Cursor not found." });

                cursorGuid = parsed;
            }

            var query = db.Vehicles.AsQueryable();

            if (cursorGuid.HasValue)
            {
                query = query.Where(y => y.Id.CompareTo(cursorGuid.Value) > 0);
            }

            query = query.OrderBy(y => y.Id).Take(limit + 1);

            var vehicles = await query.ToListAsync();

            bool hasNext = vehicles.Count > limit;
            Guid? nextCursor = null;

            if (hasNext)
            {
                nextCursor = vehicles.Last().Id;
                vehicles = vehicles.Take(limit).ToList();
            }

            var responseItems = vehicles
                .Select(v => new ResponseItem(v.Id, v.Plate, v.Model.ToString(), v.OwnerId))
                .ToList();

            var response = new Response(
                responseItems,
                new Common.Response.PageInfo(nextCursor, hasNext),
                responseItems.Count
            );

            return Results.Ok(response);
        }
    }
}

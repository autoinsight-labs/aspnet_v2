using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Vehicles.Get
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapVehicleGetEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/{vehicleId}", HandleAsync)
                .WithSummary("Get a vehicle by id")
                .WithDescription(
                    "Returns the detailed information of a vehicle that belongs to the specified yard, including the current assignee when available." +
                    "\n\n**Path Parameters:**\n" +
                    "- `yardId` (UUID, required): Yard that owns the vehicle.\n" +
                    "- `vehicleId` (UUID, required): Vehicle identifier." +
                    "\n\n**Responses:**\n" +
                    "- `200 OK`: Vehicle found and returned.\n" +
                    "- `400 Bad Request`: Invalid yardId or vehicleId.\n" +
                    "- `404 Not Found`: Yard or vehicle not found." +
                    "\n\n**Example Response (200):**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"id\": \"3fd7b234-11aa-44f5-9a0a-0c6d9ad54a6f\",\n" +
                    "  \"plate\": \"ABC1D23\",\n" +
                    "  \"model\": \"MottuSport110i\",\n" +
                    "  \"status\": \"Waiting\",\n" +
                    "  \"enteredAt\": \"2025-11-07T10:15:32Z\",\n" +
                    "  \"leftAt\": null,\n" +
                    "  \"assignee\": {\n" +
                    "    \"id\": \"7fbd32a2-1b78-4a2e-bf53-83f1c1fdd92b\",\n" +
                    "    \"name\": \"João Lima\",\n" +
                    "    \"imageUrl\": null,\n" +
                    "    \"role\": \"Member\",\n" +
                    "    \"userId\": \"21e8c4e4-d38b-47cf-8022-f4bbf2d5f212\"\n" +
                    "  }\n" +
                    "}\n" +
                    "```"
                )
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string yardId, string vehicleId)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
            {
                return Results.BadRequest(new { error = "YardId must be a valid UUID." });
            }

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsedYardId);
            if (yard is null)
                return Results.NotFound(new { error = "Yard not found" });

            if (!Guid.TryParse(vehicleId, out var parsedVehicleId))
                return Results.BadRequest(new { error = "'Vehicle Id' must be a valid UUID." });

            var vehicle = await db.Vehicles.Include(v => v.Assignee)
                .FirstOrDefaultAsync(y => y.Id == parsedVehicleId);

            if (vehicle is null)
                return Results.NotFound(new { error = "Vehicle not found" });

            var response = new Response(
                vehicle.Id,
                vehicle.Plate,
                vehicle.Model.ToString(),
                vehicle.Status.ToString(),
                vehicle.EnteredAt,
                vehicle.LeftAt,
                vehicle.Assignee is not null ? new AssigneeResponse(
                        vehicle.Assignee.Id,
                        vehicle.Assignee.Name,
                        vehicle.Assignee.ImageUrl,
                        vehicle.Assignee.Role.ToString(),
                        vehicle.Assignee.UserId
                ) : null
            );
            return Results.Ok(response);
        }
    }
}

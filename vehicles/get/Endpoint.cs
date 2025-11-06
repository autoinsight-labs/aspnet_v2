// using AutoInsight.Data;
// using Microsoft.EntityFrameworkCore;
//
// namespace AutoInsight.Vehicles.Get
// {
//     public static class Endpoint
//     {
//         public static RouteGroupBuilder MapVehicleGetEndpoint(this RouteGroupBuilder group)
//         {
//             group.MapGet("/{vehicleId}", HandleAsync)
//                 .WithSummary("Get a Vehicle by ID")
//                 .WithDescription(
//                     "Retrieves a single Vehicle by its UUID.\n\n" +
//                     "**Route Parameter:**\n" +
//                     "- `vehicleId` (UUID, required): The ID of the vehicle to retrieve.\n\n" +
//                     "**Example Request:**\n" +
//                     "```bash\n" +
//                     "GET /v2/vehicles/6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\n" +
//                     "```\n\n" +
//                     "**Possible Responses:**\n" +
//                     "- `200 OK`: Returns the Vehicle details.\n" +
//                     "- `400 Bad Request`: Invalid UUID format.\n" +
//                     "- `404 Not Found`: Vehicle with the given ID does not exist.\n\n" +
//                     "**Example Response (200):**\n" +
//                     "```json\n" +
//                     "{\n" +
//                     "  \"id\": \"6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\",\n" +
//                     "  \"plate\": \"ABC1234\",\n" +
//                     "  \"model\": \"MottuSport110i\",\n" +
//                     "  \"ownerId\": \"d5a90c87-fb15-4df7-86f3-982b6b8e53d1\"\n" +
//                     "}\n" +
//                     "```"
//                 )
//                 .Produces<Response>(StatusCodes.Status200OK)
//                 .Produces(StatusCodes.Status400BadRequest)
//                 .Produces(StatusCodes.Status404NotFound);
//
//             return group;
//         }
//
//         private static async Task<IResult> HandleAsync(AppDbContext db, string vehicleId)
//         {
//             Guid parsed;
//             if (string.IsNullOrEmpty(vehicleId) || !Guid.TryParse(vehicleId, out parsed))
//                 return Results.BadRequest(new { error = "'Vehicle Id' must be a valid UUID." });
//
//             var vehicle = await db.Vehicles
//                 .FirstOrDefaultAsync(y => y.Id == parsed);
//
//             if (vehicle is null)
//                 return Results.NotFound(new { error = "Vehicle not found" });
//
//             var response = new Response(vehicle.Id, vehicle.Plate, vehicle.Model.ToString(), vehicle.OwnerId);
//             return Results.Ok(response);
//         }
//     }
// }

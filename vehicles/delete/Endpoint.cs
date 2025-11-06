// using AutoInsight.Data;
// using Microsoft.EntityFrameworkCore;
//
// namespace AutoInsight.Vehicles.Delete
// {
//     public static class Endpoint
//     {
//         public static RouteGroupBuilder MapVehicleDeleteEndpoint(this RouteGroupBuilder group)
//         {
//             group.MapDelete("/{vehicleId}", HandleAsync)
//                 .WithSummary("Delete a Vehicle by ID")
//                 .WithDescription(
//                     "Deletes a single Vehicle identified by its UUID.\n\n" +
//                     "**Route Parameter:**\n" +
//                     "- `vehicleId` (UUID, required): The ID of the vehicle to delete.\n\n" +
//                     "**Example Request:**\n" +
//                     "```bash\n" +
//                     "DELETE /v2/vehicles/6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\n" +
//                     "```\n\n" +
//                     "**Possible Responses:**\n" +
//                     "- `200 OK`: Vehicle successfully deleted.\n" +
//                     "- `400 Bad Request`: Invalid UUID format.\n" +
//                     "- `404 Not Found`: Vehicle with the given ID does not exist.\n\n" +
//                     "**Example Response (200):**\n" +
//                     "```json\n" +
//                     "{\n" +
//                     "  \"id\": \"6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\"\n" +
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
//             var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.Id == parsed);
//
//             if (vehicle is null)
//             {
//                 return Results.NotFound(new { error = "Vehicle not found" });
//             }
//
//             db.Vehicles.Remove(vehicle);
//             await db.SaveChangesAsync();
//
//             return Results.Ok(new Response(vehicle.Id));
//         }
//     }
// }

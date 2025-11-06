// using AutoInsight.Data;
// using FluentValidation;
// using Microsoft.EntityFrameworkCore;
//
// namespace AutoInsight.Vehicles.Update
// {
//     public static class Endpoint
//     {
//         public static RouteGroupBuilder MapVehicleUpdateEndpoint(this RouteGroupBuilder group)
//         {
//             group.MapPatch("/{vehicleId}", HandleAsync)
//                 .WithSummary("Update Vehicle properties")
//                 .WithDescription(
//                     "Updates properties of a single Vehicle identified by its UUID.\n\n" +
//                     "**Route Parameter:**\n" +
//                     "- `vehicleId` (UUID, required): The ID of the vehicle to update.\n\n" +
//                     "**Request Body:**\n" +
//                     "- `ownerId` (UUID, optional): New owner ID to assign.\n\n" +
//                     "**Example Request:**\n" +
//                     "```bash\n" +
//                     "PATCH /v2/vehicles/6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\n" +
//                     "Content-Type: application/json\n" +
//                     "\n" +
//                     "{ \"ownerId\": \"d5a90c87-fb15-4df7-86f3-982b6b8e53d1\" }\n" +
//                     "```\n\n" +
//                     "**Possible Responses:**\n" +
//                     "- `200 OK`: Vehicle successfully updated.\n" +
//                     "- `400 Bad Request`: Invalid UUID or validation failed.\n" +
//                     "- `404 Not Found`: Vehicle with the given ID does not exist.\n\n" +
//                     "**Example Response (200):**\n" +
//                     "```json\n" +
//                     "{\n" +
//                     "  \"id\": \"6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\",\n" +
//                     "  \"plate\": \"ABC-1234\",\n" +
//                     "  \"model\": \"Sedan\",\n" +
//                     "  \"ownerId\": \"d5a90c87-fb15-4df7-86f3-982b6b8e53d1\"\n" +
//                     "}\n" +
//                     "```"
//                 )
//                 .Produces<Response>(StatusCodes.Status200OK)
//                 .Produces(StatusCodes.Status400BadRequest)
//                 .Produces(StatusCodes.Status404NotFound);
//             return group;
//         }
//
//         private class Validator : AbstractValidator<Request>
//         {
//             public Validator()
//             {
//                 When(x => x.OwnerId is not null, () =>
//                 {
//                     RuleFor(x => x.OwnerId)
//                                 .NotEmpty()
//                                 .Must(id => Guid.TryParse(id, out _))
//                                 .WithMessage("'Owner Id' is not a valid UUID");
//                 });
//             }
//         }
//
//         private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string vehicleId)
//         {
//             if (!Guid.TryParse(vehicleId, out var parsedId))
//                 return Results.BadRequest(new { error = "'Vehicle Id' must be a valid UUID." });
//
//             var validation = await new Validator().ValidateAsync(request);
//             if (!validation.IsValid)
//             {
//                 return Results.ValidationProblem(validation.ToDictionary());
//             }
//
//             var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.Id == parsedId);
//             if (vehicle is null)
//             {
//                 return Results.NotFound(new { error = "Vehicle not found" });
//             }
//
//             if (request.OwnerId is not null) vehicle.OwnerId = Guid.Parse(request.OwnerId);
//
//             await db.SaveChangesAsync();
//
//             var response = new Response(vehicle.Id, vehicle.Plate, vehicle.Model.ToString(), vehicle.OwnerId);
//             return Results.Ok(response);
//         }
//     }
// }

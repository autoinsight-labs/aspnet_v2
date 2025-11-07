using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Vehicles.Update
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapVehicleUpdateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPatch("/{vehicleId}", HandleAsync)
                .WithSummary("Update a vehicle status or assignee")
                .WithDescription(
                    "Updates the status and/or assignee of a vehicle in the specified yard. Vehicles that are `Cancelled` or `Finished` cannot be updated." +
                    "\n\n**Path Parameters:**\n" +
                    "- `yardId` (UUID, required): Yard that owns the vehicle.\n" +
                    "- `vehicleId` (UUID, required): Vehicle identifier." +
                    "\n\n**Request Body (partial):**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"status\": \"OnService\",\n" +
                    "  \"assigneeId\": \"7fbd32a2-1b78-4a2e-bf53-83f1c1fdd92b\"\n" +
                    "}\n" +
                    "```" +
                    "\n\n**Responses:**\n" +
                    "- `200 OK`: Vehicle successfully updated.\\n" +
                    "- `400 Bad Request`: Invalid identifiers, request payload or forbidden state transition (including validation errors).\\n" +
                    "- `404 Not Found`: Yard, vehicle or assignee not found." +
                    "\n\n**Example Response (200):**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"id\": \"3fd7b234-11aa-44f5-9a0a-0c6d9ad54a6f\",\n" +
                    "  \"plate\": \"ABC1D23\",\n" +
                    "  \"model\": \"MottuSport110i\",\n" +
                    "  \"status\": \"OnService\",\n" +
                    "  \"enteredAt\": \"2025-11-07T10:15:32Z\",\n" +
                    "  \"leftAt\": null,\n" +
                    "  \"assigneeId\": \"7fbd32a2-1b78-4a2e-bf53-83f1c1fdd92b\"\n" +
                    "}\n" +
                    "```"
                )
                .Produces<Response>(StatusCodes.Status200OK)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                When(x => x.AssigneeId is not null, () =>
                {
                    RuleFor(x => x.AssigneeId)
                                .NotEmpty()
                                .Must(id => Guid.TryParse(id, out _))
                                .WithMessage("'Assignee Id' is not a valid UUID");
                });

                When(x => x.Status is not null, () =>
                {
                    RuleFor(x => x.Status)
                                .NotEmpty()
                                .Must(BeAValidStatus);
                });
            }

            private bool BeAValidStatus(string? status) =>
                            Enum.TryParse<VehicleStatus>(status, true, out _);
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string yardId, string vehicleId)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
                return Results.BadRequest(new { error = "'Yard Id' must be a valid UUID." });

            var yard = await db.Yards.FirstOrDefaultAsync(v => v.Id == parsedYardId);
            if (yard is null)
                return Results.NotFound(new { error = "Yard not found" });

            if (!Guid.TryParse(vehicleId, out var parsedVehicleId))
                return Results.BadRequest(new { error = "'Vehicle Id' must be a valid UUID." });

            var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.Id == parsedVehicleId);
            if (vehicle is null)
                return Results.NotFound(new { error = "Vehicle not found" });

            if (vehicle.Status == VehicleStatus.Cancelled || vehicle.Status == VehicleStatus.Finished)
                return Results.BadRequest(new { error = $"Can't update a service that has been {vehicle.Status.ToString()}" });

            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            if (request.AssigneeId is not null)
            {
                var parsedAssigneeId = Guid.Parse(request.AssigneeId);
                var assignee = await db.YardEmployees.FirstOrDefaultAsync(v => v.Id == parsedAssigneeId);

                if (assignee is null)
                    return Results.NotFound(new { error = "Assignee not found" });

                vehicle.AssigneeId = Guid.Parse(request.AssigneeId);
                vehicle.Assignee = assignee;
            }
            if (request.Status is not null) vehicle.Status = Enum.Parse<VehicleStatus>(request.Status);

            if (vehicle.Status == VehicleStatus.Cancelled || vehicle.Status == VehicleStatus.Finished)
                vehicle.LeftAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var response = new Response(
                        vehicle.Id,
                        vehicle.Plate,
                        vehicle.Model.ToString(),
                        vehicle.Status.ToString(),
                        vehicle.EnteredAt,
                        vehicle.LeftAt,
                        vehicle.AssigneeId
                );
            return Results.Ok(response);
        }
    }
}

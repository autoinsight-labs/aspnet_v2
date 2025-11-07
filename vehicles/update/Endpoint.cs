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
            group.MapPatch("/{vehicleId}", HandleAsync);

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

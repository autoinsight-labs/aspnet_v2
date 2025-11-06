using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Vehicles.Get
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapVehicleGetEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/{vehicleId}", HandleAsync);

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

using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.YardEmployees.Delete
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardEmployeeDeleteEndpoint(this RouteGroupBuilder group)
        {
            group.MapDelete("/{employeeId}", HandleAsync);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string yardId, string employeeId)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
            {
                return Results.BadRequest(new { error = "YardId must be a valid UUID." });
            }

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsedYardId);
            if (yard is null)
                return Results.NotFound(new { error = "Yard not found" });

            Guid parsedEmployeeId;
            if (string.IsNullOrEmpty(employeeId) || !Guid.TryParse(employeeId, out parsedEmployeeId))
                return Results.BadRequest(new { error = "'Employee Id' must be a valid UUID." });

            var vehicle = await db.YardEmployees.FirstOrDefaultAsync(v => v.Id == parsedEmployeeId);

            if (vehicle is null)
            {
                return Results.NotFound(new { error = "YardEmployee not found" });
            }

            db.YardEmployees.Remove(vehicle);
            await db.SaveChangesAsync();

            return Results.Ok(new Response(vehicle.Id));
        }
    }
}

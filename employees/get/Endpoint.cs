using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.YardEmployees.Get
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardEmployeeGetEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/{employeeId}", HandleAsync);

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

            var employee = await db.YardEmployees
                .FirstOrDefaultAsync(y => y.Id == parsedEmployeeId);

            if (employee is null)
                return Results.NotFound(new { error = "Employee not found" });

            var response = new Response(employee.Id, employee.Name, employee.ImageUrl, employee.Role.ToString(), employee.UserId);
            return Results.Ok(response);
        }
    }
}

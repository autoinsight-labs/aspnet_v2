using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Yards.Get
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardGetEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/{yardId}", HandleAsync);
            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string yardId)
        {
            Guid parsed;
            if (string.IsNullOrEmpty(yardId) || !Guid.TryParse(yardId, out parsed))
                return Results.BadRequest(new { error = "'Yard Id' must be a valid UUID." });

            var yard = await db.Yards
                            .Include(x => x.Employees)
                            .Include(x => x.Invites)
                            .FirstOrDefaultAsync(y => y.Id == parsed);

            if (yard is null)
            {
                return Results.NotFound(new { error = "Yard not found" });
            }

            var response = new Response(yard.Id, yard.Name, yard.OwnerId, yard.Employees, yard.Invites);

            return Results.Ok(response);
        }
    }
}

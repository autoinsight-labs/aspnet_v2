using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Yards.Delete
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardDeleteEndpoint(this RouteGroupBuilder group)
        {
            group.MapDelete("/{yardId}", HandleAsync);
            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string yardId)
        {
            Guid parsed;
            if (string.IsNullOrEmpty(yardId) || !Guid.TryParse(yardId, out parsed))
                return Results.BadRequest(new { error = "'Yard Id' must be a valid UUID." });

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsed);

            if (yard is null)
            {
                return Results.NotFound(new { error = "Yard not found" });
            }

            db.Yards.Remove(yard);
            await db.SaveChangesAsync();

            return Results.Ok(new Response(yard.Id));
        }
    }
}

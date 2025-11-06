using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Vehicles.List
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapVehicleListEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/", HandleAsync);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string yardId, string? cursor = null, int limit = 10)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
            {
                return Results.BadRequest(new { error = "YardId must be a valid UUID." });
            }

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsedYardId);
            if (yard is null)
                return Results.NotFound(new { error = "Yard not found" });

            if (limit <= 0 || limit > 100)
                return Results.BadRequest(new { error = "Limit must be between 1 and 100." });

            Guid? cursorGuid = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                if (!Guid.TryParse(cursor, out var parsed))
                    return Results.BadRequest(new { error = "Cursor must be a valid UUID." });

                bool exists = await db.Vehicles.AnyAsync(y => y.Id == parsed);
                if (!exists)
                    return Results.BadRequest(new { error = "Cursor not found." });

                cursorGuid = parsed;
            }

            var query = db.Vehicles.AsQueryable().Where(v => v.YardId == parsedYardId);

            if (cursorGuid.HasValue)
            {
                query = query.Where(y => y.Id.CompareTo(cursorGuid.Value) > 0);
            }

            query = query.OrderBy(y => y.Id).Take(limit + 1);

            var vehicles = await query.ToListAsync();

            bool hasNext = vehicles.Count > limit;
            Guid? nextCursor = null;

            if (hasNext)
            {
                nextCursor = vehicles.Last().Id;
                vehicles = vehicles.Take(limit).ToList();
            }

            var responseItems = vehicles
                .Select(v => new ResponseItem(
                            v.Id,
                            v.Plate,
                            v.Model.ToString(),
                            v.Status.ToString(),
                            v.EnteredAt,
                            v.LeftAt,
                            v.AssigneeId
                    ))
                .ToList();

            var response = new Response(
                responseItems,
                new Common.Response.PageInfo(nextCursor, hasNext),
                responseItems.Count
            );

            return Results.Ok(response);
        }
    }
}

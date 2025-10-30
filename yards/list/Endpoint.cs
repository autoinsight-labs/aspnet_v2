using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Yards.List
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardListEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/", HandleAsync);
            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string? cursor = null, int limit = 10)
        {
            if (limit <= 0 || limit > 100)
                return Results.BadRequest(new { error = "Limit must be between 1 and 100." });

            Guid? cursorGuid = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                if (!Guid.TryParse(cursor, out var parsed))
                    return Results.BadRequest(new { error = "Cursor must be a valid UUID." });

                bool exists = await db.Yards.AnyAsync(y => y.Id == parsed);
                if (!exists)
                    return Results.BadRequest(new { error = "Cursor not found." });

                cursorGuid = parsed;
            }

            var query = db.Yards.AsQueryable();

            if (cursorGuid.HasValue)
            {
                query = query.Where(y => y.Id.CompareTo(cursorGuid.Value) > 0);
            }

            query = query.OrderBy(y => y.Id).Take(limit + 1);

            var yards = await query.ToListAsync();

            bool hasNext = yards.Count > limit;
            Guid? nextCursor = null;

            if (hasNext)
            {
                nextCursor = yards.Last().Id;
                yards = yards.Take(limit).ToList();
            }

            var responseItems = yards
                .Select(y => new ResponseItem(y.Id, y.Name, y.OwnerId))
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

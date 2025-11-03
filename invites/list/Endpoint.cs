using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.List
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteListEndpoint(this RouteGroupBuilder group)
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

                bool exists = await db.YardEmployees.AnyAsync(y => y.Id == parsed);
                if (!exists)
                    return Results.BadRequest(new { error = "Cursor not found." });

                cursorGuid = parsed;
            }

            var query = db.EmployeeInvites.AsQueryable().Where(i => i.YardId == parsedYardId);

            if (cursorGuid.HasValue)
            {
                query = query.Where(y => y.Id.CompareTo(cursorGuid.Value) > 0);
            }

            query = query.OrderBy(y => y.Id).Take(limit + 1);

            var invites = await query.ToListAsync();

            bool hasNext = invites.Count > limit;
            Guid? nextCursor = null;

            if (hasNext)
            {
                nextCursor = invites.Last().Id;
                invites = invites.Take(limit).ToList();
            }

            var responseItems = invites
                .Select(e => new ResponseItem(e.Id, e.Email, e.Role.ToString(), e.Status.ToString(), e.CreatedAt, e.AcceptedAt, e.InviterId))
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

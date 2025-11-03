using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.ListUser
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteListUserEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/user/{email}", HandleAsync);

            return group;
        }

        private static async Task<IResult> HandleAsync(AppDbContext db, string email, string? cursor = null, int limit = 10)
        {
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

            var query = db.EmployeeInvites.Include(i => i.Yard).AsQueryable().Where(i => i.Email == email);

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
                .Select(i => new ResponseItem(i.Id, i.Email, i.Role.ToString(), i.Status.ToString(), i.CreatedAt, i.AcceptedAt, i.InviterId, new YardResponse(i.Yard.Id, i.Yard.Name)))
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

using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.Accept
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteAcceptEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/{inviteId}/accept", HandleAsync);

            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                When(x => x.ImageUrl is not null, () =>
                {
                    RuleFor(x => x.ImageUrl)
                        .NotEmpty()
                        .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                        .WithMessage("'ImageUrl' must be a valid URL.");
                });

                RuleFor(x => x.UserId).NotEmpty().Must(id => Guid.TryParse(id, out _)).WithMessage("'User Id' is not a valid UUID");
            }
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string inviteId)
        {
            if (!Guid.TryParse(inviteId, out var parsedInviteId))
            {
                return Results.BadRequest(new { error = "InviteId must be a valid UUID." });
            }

            var invite = await db.EmployeeInvites
                        .Include(i => i.Yard)
                        .FirstOrDefaultAsync(y => y.Id == parsedInviteId);
            if (invite is null)
                return Results.NotFound(new { error = "Yard not found" });

            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            invite.AcceptedAt = DateTime.UtcNow;
            invite.Status = InviteStatus.Accepted;

            var employee = new YardEmployee
            {
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                Role = invite.Role,
                UserId = Guid.Parse(request.UserId),
                YardId = invite.YardId,
                Yard = invite.Yard
            };

            db.YardEmployees.Add(employee);

            await db.SaveChangesAsync();

            return Results.Ok();
        }
    }
}

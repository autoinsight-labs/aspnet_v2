using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.EmployeeInvites.Create
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapEmployeeInviteCreateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/", HandleAsync);

            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Email).EmailAddress();
                RuleFor(x => x.Role).NotEmpty().Must(BeAValidRole)
                                .WithMessage("Role must be one of: Admin, Member"); ;
                RuleFor(x => x.InviterId).NotEmpty().Must(id => Guid.TryParse(id, out _)).WithMessage("'Inviter Id' is not a valid UUID");
            }

            private bool BeAValidRole(string role) =>
                            Enum.TryParse<EmployeeRole>(role, true, out _);
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string yardId)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
            {
                return Results.BadRequest(new { error = "YardId must be a valid UUID." });
            }

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsedYardId);
            if (yard is null)
                return Results.NotFound(new { error = "Yard not found" });

            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var invite = new EmployeeInvite
            {
                Email = request.Email,
                Role = Enum.Parse<EmployeeRole>(request.Role, true),
                InviterId = Guid.Parse(request.InviterId),
                YardId = parsedYardId,
                Yard = yard,
            };

            db.EmployeeInvites.Add(invite);

            await db.SaveChangesAsync();

            var response = new Response(invite.Id,
                        invite.Email,
                        invite.Role.ToString(),
                        invite.Status.ToString(),
                        invite.CreatedAt,
                        invite.AcceptedAt,
                        invite.InviterId,
                        new YardResponse(yard.Id, yard.Name)
                    );
            return Results.Created($"/v2/yards/{yardId}/invites/{invite.Id}", response);
        }
    }
}

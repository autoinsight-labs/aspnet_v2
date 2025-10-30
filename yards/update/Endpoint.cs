using AutoInsight.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Yards.Update
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardUpdateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPatch("/{yardId}", HandleAsync);
            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                When(x => x.Name is not null, () =>
                {
                    RuleFor(x => x.Name)
                                        .NotEmpty()
                                        .WithMessage("Name cannot be empty");
                });
                When(x => x.OwnerId is not null, () =>
                {
                    RuleFor(x => x.OwnerId)
                                        .NotEmpty()
                                        .Must(id => Guid.TryParse(id, out _))
                                        .WithMessage("'Owner Id' is not a valid UUID");
                });
            }
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string yardId)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
                return Results.BadRequest(new { error = "'Yard Id' must be a valid UUID." });

            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsedYardId);
            if (yard is null)
            {
                return Results.NotFound(new { error = "Yard not found" });
            }

            if (request.Name is not null) yard.Name = request.Name;
            if (request.OwnerId is not null) yard.OwnerId = Guid.Parse(request.OwnerId);

            await db.SaveChangesAsync();

            var response = new Response(yard.Id, yard.Name, yard.OwnerId);
            return Results.Ok(response);
        }
    }
}

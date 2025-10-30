using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;

namespace AutoInsight.Yards.Create
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardCreateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/", HandleAsync);
            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.OwnerId).NotEmpty().Must(id => Guid.TryParse(id, out _)).WithMessage("'Owner Id' is not a valid UUID");
            }
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db)
        {
            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var yard = new Yard
            {
                Name = request.Name,
                OwnerId = Guid.Parse(request.OwnerId)
            };

            db.Yards.Add(yard);

            await db.SaveChangesAsync();

            var response = new Response(yard.Id, yard.Name, yard.OwnerId);
            return Results.Created($"/v2/yards/{yard.Id}", response);
        }
    }
}

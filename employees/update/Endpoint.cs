using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.YardEmployees.Update
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapYardEmployeeUpdateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPatch("/{employeeId}", HandleAsync);
            return group;
        }

        public record Request(string? Name, string? ImageUrl, string? Role);
        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                When(x => x.Name is not null, () =>
                {
                    RuleFor(x => x.Name)
                                    .NotEmpty();
                });

                When(x => x.ImageUrl is not null, () =>
                {
                    RuleFor(x => x.ImageUrl)
                        .NotEmpty()
                        .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                        .WithMessage("'ImageUrl' must be a valid URL.");
                });

                When(x => x.Role is not null, () =>
                {
                    RuleFor(x => x.Role).NotEmpty().Must(BeAValidRole)
                                                        .WithMessage("Model must be one of: Admin, Member"); ;
                });

            }

            public bool BeAValidRole(string? role) =>
                            Enum.TryParse<EmployeeRole>(role, true, out _);
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, string yardId, string employeeId)
        {
            if (!Guid.TryParse(yardId, out var parsedYardId))
            {
                return Results.BadRequest(new { error = "YardId must be a valid UUID." });
            }

            var yard = await db.Yards.FirstOrDefaultAsync(y => y.Id == parsedYardId);
            if (yard is null)
                return Results.NotFound(new { error = "Yard not found" });

            if (!Guid.TryParse(employeeId, out var parsedEmployeeId))
                return Results.BadRequest(new { error = "'Employee Id' must be a valid UUID." });

            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var employee = await db.YardEmployees.FirstOrDefaultAsync(v => v.Id == parsedEmployeeId);
            if (employee is null)
            {
                return Results.NotFound(new { error = "Employee not found" });
            }

            if (request.Name is not null) employee.Name = request.Name;
            if (request.ImageUrl is not null) employee.ImageUrl = request.ImageUrl;
            if (request.Role is not null) employee.Role = Enum.Parse<EmployeeRole>(request.Role);

            await db.SaveChangesAsync();

            var response = new Response(employee.Id, employee.Name, employee.ImageUrl, employee.Role.ToString(), employee.UserId);
            return Results.Ok(response);
        }
    }
}

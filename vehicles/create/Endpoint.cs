using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Vehicles.Create
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapVehicleCreateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/", HandleAsync);

            return group;
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Plate)
                    .NotEmpty()
                    .Matches(@"^([A-Z]{3}\d{4}|[A-Z]{3}\s?\d[A-Z]\d{2})$")
                    .WithMessage("Plate must be a valid plate (AAA1234 or ABC1C34).");
                RuleFor(x => x.Model).NotEmpty().Must(BeAValidModel)
                                .WithMessage("Model must be one of: MottuSport110i, Mottue, HondaPop110i, TVSSport110i."); ;
            }

            private bool BeAValidModel(string model) =>
                            Enum.TryParse<VehicleModel>(model, true, out _);
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

            var vehicle = new Vehicle
            {
                Plate = request.Plate,
                Model = Enum.Parse<VehicleModel>(request.Model, true),
                Status = request.AssigneeId is null ? VehicleStatus.Scheduled : VehicleStatus.Waiting,
                YardId = parsedYardId,
                Yard = yard
            };

            if (request.AssigneeId is not null)
            {
                if (!Guid.TryParse(request.AssigneeId, out var parsedAssigneeId))
                {
                    return Results.BadRequest(new { error = "AssigneeId must be a valid UUID." });
                }

                var employee = await db.YardEmployees.FirstOrDefaultAsync(y => y.Id == parsedAssigneeId);
                if (employee is null)
                    return Results.NotFound(new { error = "Employee not found" });

                vehicle.AssigneeId = parsedAssigneeId;
                vehicle.Assignee = employee;
            }

            db.Vehicles.Add(vehicle);

            await db.SaveChangesAsync();

            var response = new Response(
                    vehicle.Id,
                    vehicle.Plate,
                    vehicle.Model.ToString(),
                    vehicle.Status.ToString(),
                    vehicle.EnteredAt,
                    vehicle.LeftAt,
                    vehicle.YardId,
                    vehicle.AssigneeId
            );
            return Results.Created($"/v2/yards/{yardId}/vehicles/{vehicle.Id}", response);
        }
    }
}

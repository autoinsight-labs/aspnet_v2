using AutoInsight.Data;
using AutoInsight.Models;
using FluentValidation;

namespace AutoInsight.Vehicles.Create
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapVehicleCreateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/", HandleAsync)
                .WithSummary("Register a new vehicle")
                .WithDescription(
                    "Creates a new vehicle for an existing owner.\n\n" +
                    "**Request Body Example:**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"plate\": \"ABC1234\",\n" +
                    "  \"model\": \"MottuSport110i\",\n" +
                    "  \"ownerId\": \"6f4b1b33-23c7-4a8b-bc6a-1c2f4e3b1caa\"\n" +
                    "}\n" +
                    "```\n\n" +
                    "**Valid Vehicle Models:**\n" +
                    "- MottuSport110i\n" +
                    "- Mottue\n" +
                    "- HondaPop110i\n" +
                    "- TVSSport110i\n\n" +
                    "**Responses:**\n" +
                    "- `201 Created`: Vehicle successfully created.\n" +
                    "- `400 Bad Request`: Invalid payload."
                )
                .Produces<Response>(StatusCodes.Status201Created)
                .ProducesValidationProblem();

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
                RuleFor(x => x.OwnerId).NotEmpty().Must(id => Guid.TryParse(id, out _)).WithMessage("'Owner Id' is not a valid UUID");
            }

            private bool BeAValidModel(string model) =>
                            Enum.TryParse<VehicleModel>(model, true, out _);
        }

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db)
        {
            var validation = await new Validator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var vehicle = new Vehicle
            {
                Plate = request.Plate,
                Model = Enum.Parse<VehicleModel>(request.Model, true),
                OwnerId = Guid.Parse(request.OwnerId)
            };

            db.Vehicles.Add(vehicle);

            await db.SaveChangesAsync();

            var response = new Response(vehicle.Id, vehicle.Plate, vehicle.Model.ToString(), vehicle.OwnerId);
            return Results.Created($"/v2/vehicles/{vehicle.Id}", response);
        }
    }
}

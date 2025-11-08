using AutoInsight.Data;
using AutoInsight.Models;
using AutoInsight.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Vehicles.Create
{
    public static class Endpoint
    {
        public static RouteGroupBuilder MapVehicleCreateEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/", HandleAsync)
                .WithSummary("Create a new vehicle for a yard")
                .WithDescription(
                    "Registers a new vehicle under the specified yard. When an assignee is provided the vehicle starts in the `Waiting` status, otherwise it defaults to `Scheduled`." +
                    "\n\n**Path Parameters:**\n" +
                    "- `yardId` (UUID, required): Yard that will own the vehicle." +
                    "\n\n**Request Body:**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"plate\": \"ABC1D23\",\n" +
                    "  \"model\": \"MottuSport110i\",\n" +
                    "  \"assigneeId\": \"7fbd32a2-1b78-4a2e-bf53-83f1c1fdd92b\"\n" +
                    "}\n" +
                    "```" +
                    "\n\n**Responses:**\n" +
                    "- `201 Created`: Vehicle successfully created (returns vehicle details).\n" +
                    "- `400 Bad Request`: Invalid yardId, assigneeId or request payload.\n" +
                    "- `404 Not Found`: Yard or assignee not found." +
                    "\n\n**Example Response (201):**\n" +
                    "```json\n" +
                    "{\n" +
                    "  \"id\": \"9f1f3a93-bf6d-4028-91cb-238aaf3b2368\",\n" +
                    "  \"plate\": \"ABC1D23\",\n" +
                    "  \"model\": \"MottuSport110i\",\n" +
                    "  \"status\": \"Waiting\",\n" +
                    "  \"enteredAt\": \"2025-11-07T10:15:32Z\",\n" +
                    "  \"leftAt\": null,\n" +
                    "  \"yardId\": \"6b1b36c2-8f63-4c2b-b3df-9c5d9cfefb83\",\n" +
                    "  \"assigneeId\": \"7fbd32a2-1b78-4a2e-bf53-83f1c1fdd92b\"\n" +
                    "}\n" +
                    "```"
                )
                .Produces<Response>(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

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

        private static async Task<IResult> HandleAsync(Request request, AppDbContext db, IYardCapacitySnapshotService snapshotService, string yardId)
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

                var employee = await db.YardEmployees
                    .FirstOrDefaultAsync(y => y.Id == parsedAssigneeId && y.YardId == parsedYardId);
                if (employee is null)
                    return Results.NotFound(new { error = "Employee not found" });

                vehicle.AssigneeId = parsedAssigneeId;
                vehicle.Assignee = employee;
            }

            db.Vehicles.Add(vehicle);

            await db.SaveChangesAsync();
            await snapshotService.CaptureAsync(yard);

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

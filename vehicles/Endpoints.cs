using AutoInsight.Vehicles.Create;
using AutoInsight.Vehicles.List;

namespace AutoInsight.Vehicles
{
    public static class VehicleEnpoints
    {
        public static RouteGroupBuilder MapVehicleEnpoints(this RouteGroupBuilder group)
        {
            var vehicleGroup = group.MapGroup("/vehicles").WithTags("vehicle");

            vehicleGroup
                 .MapVehicleCreateEndpoint()
                 .MapVehicleListEndpoint();

            return group;
        }
    }
}

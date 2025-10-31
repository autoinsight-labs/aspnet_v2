using AutoInsight.Vehicles.Create;
using AutoInsight.Vehicles.List;
using AutoInsight.Vehicles.Get;
using AutoInsight.Vehicles.Delete;

namespace AutoInsight.Vehicles
{
    public static class VehicleEnpoints
    {
        public static RouteGroupBuilder MapVehicleEnpoints(this RouteGroupBuilder group)
        {
            var vehicleGroup = group.MapGroup("/vehicles").WithTags("vehicle");

            vehicleGroup
                 .MapVehicleCreateEndpoint()
                 .MapVehicleListEndpoint()
                 .MapVehicleGetEndpoint()
                 .MapVehicleDeleteEndpoint();

            return group;
        }
    }
}

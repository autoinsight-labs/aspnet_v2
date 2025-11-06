// using AutoInsight.Vehicles.Create;
// using AutoInsight.Vehicles.List;
// using AutoInsight.Vehicles.Get;
// using AutoInsight.Vehicles.Delete;
// using AutoInsight.Vehicles.Update;
//
// namespace AutoInsight.Vehicles
// {
//     public static class VehicleEnpoints
//     {
//         public static RouteGroupBuilder MapVehicleEnpoints(this RouteGroupBuilder group)
//         {
//             var vehicleGroup = group.MapGroup("/vehicles").WithTags("vehicle");
//
//             vehicleGroup
//                  .MapVehicleCreateEndpoint()
//                  .MapVehicleListEndpoint()
//                  .MapVehicleGetEndpoint()
//                  .MapVehicleDeleteEndpoint()
//                  .MapVehicleUpdateEndpoint();
//
//             return group;
//         }
//     }
// }

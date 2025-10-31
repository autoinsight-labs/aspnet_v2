using AutoInsight.YardEmployees.List;
using AutoInsight.YardEmployees.Get;
using AutoInsight.YardEmployees.Delete;

namespace AutoInsight.YardEmployees
{
    public static class YardEmployeesEnpoints
    {
        public static RouteGroupBuilder MapYardEmployeeEnpoints(this RouteGroupBuilder group)
        {
            var employeeGroup = group.MapGroup("/yards/{yardId}/employees").WithTags("employee");

            employeeGroup
                 .MapYardEmployeeListEndpoint()
                 .MapYardEmployeeGetEndpoint()
                 .MapYardEmployeeDeleteEndpoint();

            return group;
        }
    }
}

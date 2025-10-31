using AutoInsight.YardEmployees.List;

namespace AutoInsight.YardEmployees
{
    public static class YardEmployeesEnpoints
    {
        public static RouteGroupBuilder MapYardEmployeeEnpoints(this RouteGroupBuilder group)
        {
            var employeeGroup = group.MapGroup("/yards/{yardId}/employees").WithTags("employee");

            employeeGroup
                 .MapYardEmployeeListEndpoint();

            return group;
        }
    }
}

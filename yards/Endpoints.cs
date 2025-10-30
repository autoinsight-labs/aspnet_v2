using AutoInsight.Yards.Create;

namespace AutoInsight.Yards
{
    public static class YardEnpoints
    {
        public static RouteGroupBuilder MapYardEnpoints(this RouteGroupBuilder group)
        {
            var yardGroup = group.MapGroup("/yards").WithTags("yard");

            yardGroup.MapYardCreateEndpoint();
            return yardGroup;
        }
    }
}

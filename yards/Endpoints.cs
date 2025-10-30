using AutoInsight.Yards.Create;
using AutoInsight.Yards.List;
using AutoInsight.Yards.Get;
using AutoInsight.Yards.Delete;
using AutoInsight.Yards.Update;

namespace AutoInsight.Yards
{
    public static class YardEnpoints
    {
        public static RouteGroupBuilder MapYardEnpoints(this RouteGroupBuilder group)
        {
            var yardGroup = group.MapGroup("/yards").WithTags("yard");

            yardGroup.MapYardCreateEndpoint().MapYardListEndpoint().MapYardGetEndpoint().MapYardDeleteEndpoint().MapYardUpdateEndpoint();

            return yardGroup;
        }
    }
}

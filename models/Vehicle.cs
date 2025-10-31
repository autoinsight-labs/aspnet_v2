namespace AutoInsight.Models
{
    public enum VehicleModel
    {
        MottuSport110i,
        Mottue,
        HondaPop110i,
        TVSSport110i
    }

    public class Vehicle
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string Plate { get; set; }
        public required VehicleModel Model { get; set; }
        public Guid OwnerId { get; set; }
    }
}

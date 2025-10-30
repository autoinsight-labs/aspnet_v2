namespace AutoInsight.Models
{
    public class Yard
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required string Name { get; set; }
        public Guid OwnerId { get; init; }

        public ICollection<YardEmployee> Employees { get; } = new List<YardEmployee>();
        public ICollection<YardVehicle> Vehicles { get; } = new List<YardVehicle>();
    }
}

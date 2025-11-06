namespace AutoInsight.Models
{
    public class Yard
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required string Name { get; set; }
        public Guid OwnerId { get; set; }

        public ICollection<YardEmployee> Employees { get; } = new List<YardEmployee>();
        public ICollection<Vehicle> Vehicles { get; } = new List<Vehicle>();
        public ICollection<EmployeeInvite> Invites { get; } = new List<EmployeeInvite>();
    }
}

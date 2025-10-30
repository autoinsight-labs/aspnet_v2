namespace AutoInsight.Models
{
    public enum VehicleStatus
    {
        Scheduled,
        Waiting,
        OnService,
        Finished,
        Cancelled
    }

    public class YardVehicle
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required VehicleStatus Status { get; set; }
        public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }

        public required Guid VehicleId { get; init; }
        public required Vehicle Vehicle { get; set; }

        public required Guid YardId { get; init; }
        public required Yard Yard { get; set; }
    }
}

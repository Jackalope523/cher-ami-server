namespace Repository
{
    public class GatheringReport : Entity
    {
        public GatheringReportType Type { get; set; }

        public long? UserId { get; init; }
        public long GatheringId { get; init; }
        public DateTimeOffset FilingDate { get; init; }
        public string Notes { get; init; }

        // Navigation Properties
        public User? User { get; init; }
        public Gathering? Gathering { get; init; }

        // Default Values
    }
}

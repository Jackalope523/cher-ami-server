namespace Repository
{
    public class UserReport : Entity
    {
        public UserReportType Type { get; set; }

        public long? SelfId { get; init; }
        public long OtherId { get; init; }
        public long? GatheringId { get; init; }
        public DateTimeOffset FilingDate { get; init; }
        public string Notes { get; init; }

        // Navigation Properties
        public User? Self { get; init; }
        public User? Other { get; init; }
        public Gathering? Gathering { get; init; }

        // Default Values
    }
}

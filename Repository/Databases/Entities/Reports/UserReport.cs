namespace Repository.Databases.Entities.Reports
{
    public class UserReport : Report
    {
        public UserReportType Type { get; set; }

        public long OtherId { get; init; }
        public long? GatheringId { get; init; }

        // Navigation Properties
        public User? Other { get; init; }
        public Circle? Gathering { get; init; }
    }
}

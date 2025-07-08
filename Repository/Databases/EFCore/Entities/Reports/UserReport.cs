namespace Repository.Databases.EFCore.Entities.Reports
{
    public class UserReport : Entity
    {
        public UserReportType Type { get; set; }

        public long OtherId { get; init; }
        public long? GatheringId { get; init; }

        // Navigation Properties
        public User? Other { get; init; }
        public Gathering? Gathering { get; init; }
    }
}

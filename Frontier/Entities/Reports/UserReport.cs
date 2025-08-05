using Repository.Entities;

namespace Repository.Entities.Reports
{
    public class UserReport : Report
    {
        public UserReportType Type { get; set; }

        public long UserId { get; init; }
        public long? GatheringId { get; init; }

        // Navigation Properties
        public User? User { get; init; }
        public Circle? Gathering { get; init; }
    }
}

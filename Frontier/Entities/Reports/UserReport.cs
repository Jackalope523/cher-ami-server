using Core.Boundaries;

namespace CrazyLizard.Entities.Reports
{
    public class UserReport : Report
    {
        public UserReportType Type { get; set; }

        public long Id { get; set; }
        public long ReportedUserId { get; init; }
        public long? GatheringId { get; init; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User ReportedUser { get; init; }
        public Circle Gathering { get; init; }
    }
}

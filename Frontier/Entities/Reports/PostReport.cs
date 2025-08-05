using Repository.Entities;

namespace Repository.Entities.Reports
{
    public class PostReport : Report
    {
        public PostReportType Type { get; set; }

        public long PostId { get; init; }

        // Navigation Properties
        public Post? Post { get; init; }
    }
}

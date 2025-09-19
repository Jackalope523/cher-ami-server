using Core.Boundaries;

namespace CrazyLizard.Entities.Reports
{
    public class PostReport : Report
    {
        public PostReportType Type { get; set; }

        public long Id { get; set; }
        public long PostId { get; init; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public Post Post { get; init; }
    }
}

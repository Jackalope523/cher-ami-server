using Core.Boundaries;
using Repository.Entities;

namespace CrazyLizard.Entities.Reports
{
    public class PostReport : Report
    {
        public PostReportType Type { get; set; }

        public long PostId { get; init; }

        // Navigation Properties
        public Post Post { get; init; }
    }
}

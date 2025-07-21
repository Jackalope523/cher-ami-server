using Repository.Databases.Entities;

namespace Repository.Databases.Entities.Reports
{
    public class CaptionReport : Report
    {
        public enum ReportType { }

        public ReportType Type { get; set; }

        public long CaptionId { get; init; }

        // Navigation Properties
        public Caption? Caption { get; init; }
    }
}

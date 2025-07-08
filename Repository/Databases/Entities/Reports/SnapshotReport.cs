using Repository.Databases.Entities;

namespace Repository.Databases.Entities.Reports
{
    public class SnapshotReport : Entity
    {
        public PostReportType Type { get; set; }

        public long SnapshotId { get; init; }

        // Navigation Properties
        public Snapshot? Snapshot { get; init; }
    }
}

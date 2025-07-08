namespace Repository.Databases.EFCore.Entities.Reports
{
    public class SnapshotReport : Entity
    {
        public PostReportType Type { get; set; }

        public long SnapshotId { get; init; }

        // Navigation Properties
        public Snapshot? Snapshot { get; init; }
    }
}

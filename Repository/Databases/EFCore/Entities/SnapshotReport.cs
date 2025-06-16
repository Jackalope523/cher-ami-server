namespace Repository
{
    public class SnapshotReport : Entity
    {
        public SnapshotReportType Type { get; set; }

        public long? UserId { get; init; }
        public long SnapshotId { get; init; }
        public DateTimeOffset FilingDate { get; init; }
        public string Notes { get; init; }

        // Navigation Properties
        public User? User { get; init; }
        public Snapshot? Snapshot { get; init; }

        // Default Values
    }
}

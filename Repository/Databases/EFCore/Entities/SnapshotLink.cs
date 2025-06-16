namespace Repository
{
    public class SnapshotLink : Entity
    {
        public enum SnapshotLinkType { Appreciate }

        public long UserId { get; init; }
        public long SnapshotId { get; init; }
        public DateTimeOffset Time { get; init; }
        public SnapshotLinkType Type { get; set; }

        // Navigation Properties
        public User? User { get; init; }
        public Snapshot? Snapshot { get; init; }

        // Default Values
    }
}

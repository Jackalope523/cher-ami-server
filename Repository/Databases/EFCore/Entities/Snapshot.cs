namespace Repository
{
    public class Snapshot : Entity
    {
        public long OwnerId { get; set; }
        public long GatheringId { get; set; }
        public DateTimeOffset PostedAt { get; init; }

        // Navigation Properties
        public User? Owner { get; set; }
        public Gathering? Gathering { get; set; }
        public List<SnapshotReport>? Reports { get; set; }
        public List<SnapshotLink>? SnapshotLinks { get; set; }

        // Default Values

    }
}

using Repository.Entities;

namespace Repository.Entities.Messages
{
    public class SnapshotMessage : Message
    {
        public long SnapshotId { get; set; }

        // Navigation Properties
        public Snapshot? Snapshot { get; set; }
    }
}

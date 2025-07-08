using Repository.Databases.Entities;

namespace Repository.Databases.Entities.Messages
{
    public class SnapshotMessage : Message
    {
        public long SnapshotId { get; set; }

        // Navigation Properties
        public Snapshot? Snapshot { get; set; }
    }
}

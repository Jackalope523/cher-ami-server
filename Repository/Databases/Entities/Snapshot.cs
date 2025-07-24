using Repository.Databases.Entities.Reports;

namespace Repository.Databases.Entities
{
    public class Snapshot : Entity
    {
        public long PostId { get; set; }
        public int SequenceNumber { get; set; }
        public string Path { get; set; } = DefaultPath;

        // Navigation Properties
        public Post? Post { get; set; }
        public List<SnapshotReport>? Reports { get; set; }

        // Default Values
        public static string DefaultPath { get; set; } = "";

    }
}

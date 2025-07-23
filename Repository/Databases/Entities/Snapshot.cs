using Repository.Databases.Entities.Reports;

namespace Repository.Databases.Entities
{
    public class Snapshot : Entity
    {
        public long PostId { get; set; }
        public int SequenceNumber { get; set; }
        public string Filename { get; set; } = DefaultFilename;

        // Navigation Properties
        public Post? Post { get; set; }
        public List<SnapshotReport>? Reports { get; set; }

        // Default Values
        public static string DefaultFilename { get; set; } = "";

    }
}

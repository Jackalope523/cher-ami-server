using Repository.Databases.Entities.Reports;

namespace Repository.Databases.Entities
{
    public class Post : Entity
    {
        public enum LayoutType { Single, Double }

        public long AuthorId { get; init; }
        public LayoutType Layout { get; init; }

        // Navigation Properties
        public User? Author { get; set; }
        public List<Snapshot>? Snapshots { get; set; }
        public List<Caption>? Captions { get; set; }
        public List<SnapshotReport>? Reports { get; set; }

        // Default Values

    }
}

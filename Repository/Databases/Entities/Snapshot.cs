using Repository.Databases.Entities.Reports;

namespace Repository.Databases.Entities
{
    public class Snapshot : Entity
    {
        public long PostId { get; set; }

        // Navigation Properties
        public Post? Post { get; set; }
        public List<SnapshotReport>? Reports { get; set; }

        // Default Values

    }
}

namespace Repository.Entities
{
    public class Snapshot : Entity
    {
        public long PostId { get; set; }
        public int SequenceNumber { get; set; }
        public string Path { get; set; } = DefaultPath;

        // Navigation Properties
        public Post? Post { get; set; }

        // Default Values
        public static string DefaultPath { get; set; } = "";

    }
}

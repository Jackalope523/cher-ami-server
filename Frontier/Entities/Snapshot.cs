namespace CrazyLizard.Entities
{
    public class Snapshot
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public int SequenceNumber { get; set; }
        public string Path { get; set; } = DefaultPath;
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public Post Post { get; set; }

        // Default Values
        public static string DefaultPath { get; set; } = "";

    }
}

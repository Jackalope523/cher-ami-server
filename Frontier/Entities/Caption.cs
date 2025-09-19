namespace CrazyLizard.Entities
{
    public class Caption
    {
        public long Id { get; set; }
        public long PostId { get; set; }

        public int SequenceNumber { get; set; }
        public string Text { get; set; } = DefaultText;
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public Post Post { get; set; }

        // Default Values
        public static string DefaultText { get; set; } = "";
    }
}

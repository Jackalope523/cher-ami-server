namespace Repository.Entities
{
    public class Caption : Entity
    {
        public long PostId { get; set; }

        public int SequenceNumber { get; set; }
        public string Text { get; set; } = DefaultText;

        // Navigation Properties
        public Post? Post { get; set; }
        public List<CaptionReport>? Reports { get; set; }

        // Default Values
        public static string DefaultText { get; set; } = "";
    }
}

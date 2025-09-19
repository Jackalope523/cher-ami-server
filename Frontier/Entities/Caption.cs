using Entities;

namespace CrazyLizard.Entities
{
    public class Caption : Entity
    {
        public long PostId { get; set; }

        public int SequenceNumber { get; set; }
        public string Text { get; set; } = DefaultText;

        // Navigation Properties
        public Post Post { get; set; }

        // Default Values
        public static string DefaultText { get; set; } = "";
    }
}

namespace CrazyLizard.Entities
{
    internal class Word
    { 
        public enum WordType
        {
            Adjective, Noun,
        }

        public long Id { get; set; }
        public string Text { get; set; } = DefaultText;
        public WordType Type { get; set; } = DefaultType;
        public bool SoftDeleted { get; set; }

        // Default Values
        public static string DefaultText { get; set; } = "";
        public static WordType DefaultType { get; set; } = WordType.Noun;
    }
}

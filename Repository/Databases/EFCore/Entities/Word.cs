namespace Repository
{
    internal class Word : Entity
    { 
        public enum WordType
        {
            Adjective, Noun,
        }

        public string Text { get; set; } = DefaultText;
        public WordType Type { get; set; } = DefaultType;

        // Default Values
        public static string DefaultText { get; set; } = "";
        public static WordType DefaultType { get; set; } = WordType.Noun;
    }
}

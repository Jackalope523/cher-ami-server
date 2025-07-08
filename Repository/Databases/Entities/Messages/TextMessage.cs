namespace Repository.Databases.Entities.Messages
{
    public class TextMessage : Message
    {
        public string Text { get; set; } = DefaultText;

        // Default Values
        public static string DefaultText { get; set; } = "";
    }
}

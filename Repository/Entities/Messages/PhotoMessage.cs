namespace Repository.Entities.Messages
{
    public class PhotoMessage : Message
    {
        public string Path { get; set; } = DefaultPath;

        // Default Values
        public static string DefaultPath { get; set; } = "";
    }
}

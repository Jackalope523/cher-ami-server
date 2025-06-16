namespace Repository
{
    abstract public class Chat : Entity
    {
        public ChatType Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Navigation Properties
        public List<ChatLink>? ChatLinks { get; set; }
        public List<Message>? Messages { get; set; }
    }
}

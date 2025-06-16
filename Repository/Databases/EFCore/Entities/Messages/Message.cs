namespace Repository
{
    public abstract class Message : Entity
    {
        public long? UserId { get; set; }
        public long ConversationId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public MessageType Type { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Chat? Chat { get; set; }
    }
}

namespace Repository
{
    public class ChatLink : Entity
    {
        public long UserId { get; set; }
        public long ConversationId { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public DateTimeOffset? HiddenFrom { get; set; }
        public MembershipType Type { get; set; }
        public bool Muted { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Chat? Chat { get; set; }
    }
}
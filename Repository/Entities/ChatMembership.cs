using Repository.Entities.Chats;

namespace Repository.Entities
{
    public class ChatMembership : Entity
    {
        public long UserId { get; set; }
        public long ChatId { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public DateTimeOffset? HiddenFrom { get; set; }
        public ChatMembershipType Type { get; set; }
        public bool Muted { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Chat? Chat { get; set; }
    }
}
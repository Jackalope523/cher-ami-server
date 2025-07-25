using Repository.Entities;
using Repository.Entities.Chats;

namespace Repository.Entities.Messages
{
    public abstract class Message : Entity
    {
        public long? UserId { get; set; }
        public long ChatId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public MessageType Type { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Chat? Chat { get; set; }
    }
}

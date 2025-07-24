using Repository.Entities;
using Repository.Entities.Messages;

namespace Repository.Entities.Chats
{
    abstract public class Chat : Entity
    {
        public ChatType Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Navigation Properties
        public List<ChatMembership>? ChatLinks { get; set; }
        public List<Message>? Messages { get; set; }
    }
}

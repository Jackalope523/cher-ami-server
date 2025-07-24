using Repository.Entities;

namespace Repository.Entities.Chats
{
    public class CircleChat : Chat
    {
        public long CircleId { get; set; }
      

        // Navigation Properties
        public Circle? Circle { get; set; }
    }
}

using Repository.Entities;

namespace Repository.Entities.Messages
{
    public class PostMessage : Message
    {
        public long GatheringId { get; set; }

        // Navigation Properties
        public Circle? Gathering { get; set; }
    }
}

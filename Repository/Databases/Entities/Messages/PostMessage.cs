using Repository.Databases.Entities;

namespace Repository.Databases.Entities.Messages
{
    public class PostMessage : Message
    {
        public long GatheringId { get; set; }

        // Navigation Properties
        public Circle? Gathering { get; set; }
    }
}

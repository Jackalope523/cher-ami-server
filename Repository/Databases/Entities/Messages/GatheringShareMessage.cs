using Repository.Databases.Entities;

namespace Repository.Databases.Entities.Messages
{
    public class GatheringShareMessage : Message
    {
        public long GatheringId { get; set; }

        // Navigation Properties
        public Gathering? Gathering { get; set; }
    }
}

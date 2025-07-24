using Repository.Entities;

namespace Repository.Entities.Messages
{
    public class IssueMessage : Message
    {
        public long GatheringId { get; set; }

        // Navigation Properties
        public Circle? Gathering { get; set; }
    }
}

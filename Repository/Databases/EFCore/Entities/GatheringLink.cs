
namespace Repository
{
    public class GatheringLink : Entity
    {
        public long UserId { get; set; }
        public long GatheringId { get; set; }
        public DateTimeOffset Time { get; set; }
        public GatheringBond Type { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Gathering? Gathering { get; set; }

        // Default Values
    }
}

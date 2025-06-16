namespace Repository
{
    public class GatheringShareMessage : Message
    {
        public long GatheringId { get; set; }

        // Navigation Properties
        public Gathering? Gathering { get; set; }
    }
}

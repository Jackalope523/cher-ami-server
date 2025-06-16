namespace Repository
{
    public class GatheringChat : Chat
    {
        public long GatheringId { get; set; }
      

        // Navigation Properties
        public Gathering? Gathering { get; set; }
    }
}

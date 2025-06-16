namespace Repository
{
    public class ProfileMessage : Message
    {
        public long ProfileId { get; set; }

        // Navigation Properties
        public User? Profile { get; set; }
    }
}

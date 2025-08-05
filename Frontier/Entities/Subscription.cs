namespace Repository.Entities
{
    public class Subscription : Entity
    {
        public long UserId { get; set; }
        public string DeviceToken { get; set; }

        // Navigation Properties
        public User? User { get; set; }

        // Default Values
    }
}

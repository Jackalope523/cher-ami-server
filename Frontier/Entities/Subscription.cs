namespace CrazyLizard.Entities
{
    public class Subscription
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string DeviceToken { get; set; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User User { get; set; }

        // Default Values
    }
}

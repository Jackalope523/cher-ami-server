namespace Repository
{
    public class Notification : Entity
    {
        public enum NotificationType { GatheringImminent, GatheringUpcoming, GatheringWaiting }

        public long RecipientId { get; set; }
        public long GatheringId { get; set; }
        public string NotificationId { get; set; }
        public NotificationType Type { get; set; }


        // Navigation Properties
        public User? Recipient { get; set; }
        public Gathering? Gathering { get; set; }

        // Default Values
    }
}

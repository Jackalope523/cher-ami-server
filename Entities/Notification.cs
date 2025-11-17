namespace CherAmiAPI.Entities
{
    public class Notification
    {
        public long Id { get; set; }
        public enum NotificationType { GatheringImminent, GatheringUpcoming, GatheringWaiting }

        public long RecipientId { get; set; }
        public long GatheringId { get; set; }
        public string NotificationId { get; set; }
        public NotificationType Type { get; set; }
        public bool SoftDeleted { get; set; }


        // Navigation Properties
        public User Recipient { get; set; }
        public Circle Circles { get; set; }

        // Default Values
    }
}

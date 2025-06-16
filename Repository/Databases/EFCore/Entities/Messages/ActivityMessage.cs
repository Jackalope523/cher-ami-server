namespace Repository
{
    public class ActivityMessage : Message
    { 
        public ActivityMessageType ActivityType { get; set; }
        public long? ActorId { get; set; }
        public long? TargetId { get; set; }
        public string Text { get; set; }

        //Navigation Properties
        public User? Actor { get; set; }
        public User? Target { get; set; }
    }
}
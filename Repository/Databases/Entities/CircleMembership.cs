namespace Repository.Databases.Entities
{
    public class CircleMembership : Entity
    {
        public long UserId { get; set; }
        public long CircleId { get; set; }
        public DateTimeOffset Time { get; set; }
        public CircleMembershipType Type { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Circle? Circle { get; set; }

        // Default Values
    }
}

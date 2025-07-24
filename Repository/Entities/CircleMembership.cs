namespace Repository.Entities
{
    public class CircleMembership : Entity
    {
        public long UserId { get; set; }
        public long CircleId { get; set; }
        public DateTimeOffset JoinDate { get; set; }
        public CircleMembershipType Type { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Circle? Circle { get; set; }

        // Default Values
    }
}

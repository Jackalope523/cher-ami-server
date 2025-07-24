namespace Repository.Databases.Entities
{
    public class Block : Entity
    {
        public long BlockerId { get; set; }
        public long BlockedId { get; set; }
        public DateTimeOffset BlockDate { get; set; }

        // Navigation Properties
        public User? Blocker { get; set; }
        public User? Blocked { get; set; }
    }
}

namespace Repository
{
    public class Connection : Entity
    {
        public long UserId { get; set; }
        public string ConnectionId { get; set; } = DefaultConnectionId;

        // Navigation Properties
        public User? User { get; set; }

        // Default Values
        public static string DefaultConnectionId = string.Empty;
    }
}

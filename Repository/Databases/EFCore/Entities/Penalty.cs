namespace Repository
{
    public class Penalty : Entity
    {
        public long PenalizedId { get; set; }
        public PenaltyType Type { get; set; }   
        public DateTimeOffset Time { get; set; }

        // Navigation Properties
        public User? Penalized { get; set; }

        // Default Values
    }
}

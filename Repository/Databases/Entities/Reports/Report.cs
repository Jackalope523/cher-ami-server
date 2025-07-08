using Repository.Databases.Entities;

namespace Repository.Databases.Entities.Reports
{
    public abstract class Report : Entity
    {
        public long? FilingUserId { get; init; }
        public DateTimeOffset FilingDate { get; init; }
        public string Notes { get; init; }

        // Navigation Properties
        public User? FilingUser { get; init; }
    }
}

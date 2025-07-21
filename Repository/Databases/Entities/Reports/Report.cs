namespace Repository.Databases.Entities.Reports
{
    public abstract class Report : Entity
    {
        public enum ReportDiscriminator 
        { 
            UserReport,
            SnapshotReport,
            CaptionReport
        }

        public ReportDiscriminator Discriminator { get; init; }
        public long? FilingUserId { get; init; }
        public DateTimeOffset FilingDate { get; init; }
        public string Notes { get; init; } = DefaultNotes;

        // Navigation Properties
        public User? FilingUser { get; init; }

        // Default Values
        public static string DefaultNotes { get; set; } = "";
    }
}

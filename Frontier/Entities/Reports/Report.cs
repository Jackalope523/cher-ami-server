using System;

namespace CrazyLizard.Entities.Reports
{
    public abstract class Report
    {
        public enum ReportDiscriminator 
        { 
            UserReport,
            PostReport,
        }

        public long Id { get; set; }
        public ReportDiscriminator Discriminator { get; init; }
        public long? FilingUserId { get; init; }
        public DateTimeOffset FilingDate { get; init; }
        public string Notes { get; init; } = DefaultNotes;
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User FilingUser { get; init; }

        // Default Values
        public static string DefaultNotes { get; set; } = "";
    }
}

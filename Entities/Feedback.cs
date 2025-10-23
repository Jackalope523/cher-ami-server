using System;

namespace CrazyLizard.Entities
{
    public class Feedback
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Comments { get; set; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User User { get; set; }

        // Default Values
    }
}

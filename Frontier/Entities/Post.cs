using System;
using System.Collections.Generic;
using PostReport = CrazyLizard.Entities.Reports.PostReport;

namespace CrazyLizard.Entities
{
    public class Post
    {
        public enum LayoutType { Single, Double }

        public long Id { get; set; }
        public long AuthorId { get; init; }
        public long IssueId { get; init; }
        public LayoutType Layout { get; init; }
        public DateTimeOffset PostedAt { get; init; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User Author { get; set; }
        public Issue Issue { get; set; }
        public List<Snapshot> Snapshots { get; set; }
        public List<Caption> Captions { get; set; }
        public List<PostReport> Reports { get; set; }

        // Default Values

    }
}

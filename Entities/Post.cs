using System;
using System.Collections.Generic;
using PostReport = CherAmiAPI.Entities.Reports.PostReport;

namespace CherAmiAPI.Entities
{
    public class Post
    {
        public long Id { get; set; }
        public long AuthorId { get; init; }
        public long IssueId { get; init; }
        public DateTimeOffset PostedAt { get; init; }
        public string ImagePath { get; set; }
        public string Caption { get; set; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User Author { get; set; }
        public Issue Issue { get; set; }
        public List<PostReport> Reports { get; set; }

        // Default Values

    }
}

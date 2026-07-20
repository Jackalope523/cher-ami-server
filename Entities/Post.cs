using System;
using System.Collections.Generic;
using PostReport = CherAmiAPI.Entities.Reports.PostReport;

namespace CherAmiAPI.Entities
{
    public class Post
    {
        public long Id { get; set; }
        public string UploadId { get; set; }
        public long AuthorId { get; set; }
        public long IssueId { get; set; }
        public DateTimeOffset PostedAt { get; set; }

        // When the photo was taken (EXIF-derived or user-set), constrained to
        // [Issue.DraftingStart, now]. PostedAt remains the upload timestamp.
        public DateTimeOffset PhotoDate { get; set; }
        public string LowResolutionImagePath { get; set; }
        public string HighResolutionImagePath { get; set; }


        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string Caption { get; set; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User Author { get; set; }
        public Issue Issue { get; set; }
        public List<PostReport> Reports { get; set; }

        // Default Values

    }
}

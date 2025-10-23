using System;
using System.Collections.Generic;

namespace CrazyLizard.Entities
{
    public enum IssueSchedule
    {
        Monthly
    }
    public enum IssueStatus
    {
        Drafting,
        Published,
        Shipped,
        Archived,
    }

    public class Issue
    {
        public long Id { get; set; }
        public long CircleId { get; set; }
        public string Title { get; set; }
        public int IssueNumber { get; set; }
        public DateTimeOffset DraftingStart { get; set; }
        public DateTimeOffset DraftingEnd { get; set; }
        public IssueStatus Status { get; set; }
        public string HeaderPath { get; set; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public Circle Circle { get; set; }
        public List<Post> Posts { get; set; }
    }
}

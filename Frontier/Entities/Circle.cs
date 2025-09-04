using Core.Boundaries;
using System;
using System.Collections.Generic;

namespace Repository.Entities
{
    public class Circle : Entity
    {
        public string Title { get; set; } = DefaultTitle;
        public DateTimeOffset TimeOfCreation { get; set; }
        public string CircleCode { get; set; } = DefaultCircleCode;
        public IssueSchedule IssueSchedule { get; set; }
        public string HeaderPath { get; set; } = DefaultHeaderPath;


        // Navigation Properties
        public List<CircleMembership> CircleMemberships { get; set; }
        public List<RecipientLink> CircleRecipients { get; set; }
        public List<Issue> Issues { get; set; }
        public List<Notification> Notifications { get; set; }

        // Default Values
        public static string DefaultTitle { get; set; } = "";
        public static string DefaultCircleCode { get; set; } = "";
        public static string DefaultHeaderPath { get; set; } = "";
        public static DateTimeOffset DefaultTimeOfCreation { get; set; } = DateTimeOffset.MinValue;
    }
}

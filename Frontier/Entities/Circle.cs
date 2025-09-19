using Core.Boundaries;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace CrazyLizard.Entities
{
    public class Circle
    {
        public long Id { get; set; }
        public string Title { get; set; } = DefaultTitle;
        public DateTimeOffset TimeOfCreation { get; set; }
        public string CircleCode { get; set; } = DefaultCircleCode;
        public IssueSchedule IssueSchedule { get; set; }
        public string HeaderPath { get; set; } = DefaultHeaderPath;
        public bool SoftDeleted { get; set; }


        // Navigation Properties
        public List<User> Members { get; set; }
        public List<Issue> Issues { get; set; }
        public List<Notification> Notifications { get; set; }

        // Default Values
        public static string DefaultTitle { get; set; } = "";
        public static string DefaultCircleCode { get; set; } = "";
        public static string DefaultHeaderPath { get; set; } = "";
        public static DateTimeOffset DefaultTimeOfCreation { get; set; } = DateTimeOffset.MinValue;
    }
}

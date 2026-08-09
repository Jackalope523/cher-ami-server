using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using PostReport = CherAmiAPI.Entities.Reports.PostReport;
using UserReport = CherAmiAPI.Entities.Reports.UserReport;

namespace CherAmiAPI.Entities
{
    public enum UserAccountStatus
    { 
        Active,
        Prospective,
    }

    public class User : IdentityUser<long>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public DateTimeOffset JoinDate { get; set; }
        public UserAccountStatus AccountStatus { get; set; }
        public DateTimeOffset TimeOfUserAgreement { get; set; }
        public string AvatarPath { get; set; }
        public DateTimeOffset? AvatarTimestamp { get; set; }
        public Guid ExternalId { get; set; }
        public string OneSignalId { get; set; }
        public string StripeCustomerId { get; set; }
        public string StripeSubscriptionId { get; set; }
        public long? CircleId { get; set; }
        public DateTimeOffset? CircleJoinDate { get; set; }
        public string GoogleId { get; set; }
        public string AppleId { get; set; }
        public bool IsBillingExempt { get; set; }
        public bool SoftDeleted { get; set; }

        // Onboarding Flags
        public bool NameProvidedByUser { get; set; }
        public bool OnboardingCompleted { get; set; }

        // Notification Profile
        public Guid? NotificationId { get; set; }
        public bool IssuePosts { get; set; } = DefaultIssuePosts;
        public bool IssueReminders { get; set; } = DefaultIssueReminders;

        // Navigation Properties
        public Circle Circle { get; set; }
        public List<UserReport> ReporterList { get; set; } = [];
        public List<UserReport> ReportedList { get; set; } = [];
        public List<Block> BlockerList { get; set; } = [];
        public List<Block> BlockedList { get; set; } = [];
        public List<PostReport> PostReports { get; set; } = [];
        public List<Subscription> Subscriptions { get; set; } = [];
        public List<Feedback> Feedback { get; set; } = [];
        public List<Notification> Notifications { get; set; } = [];
        public List<Post> Posts { get; set; } = [];
        public List<Recipient> Recipients { get; set; } = [];

        // Default Values

        // Notification Profile
        public static bool DefaultIssuePosts { get; set; } = true;
        public static bool DefaultIssueReminders { get; set; } = true;
        public static bool DefaultGatheringReminders { get; set; } = true;
        public static bool DefaultGatheringActivity { get; set; } = true;
        public static bool DefaultGatheringDiscovery { get; set; } = true;
    }
}

using Core.Boundaries;
using CrazyLizard.Contracts.Responses;
using System;
using System.Collections.Generic;
using PostReport = Repository.Entities.Reports.PostReport;
using UserReport = Repository.Entities.Reports.UserReport;

namespace Repository.Entities
{
    public class User : Entity
    {
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; init; }
        public DateTimeOffset JoinDate { get; init; }
        public bool IsPhoneConfirmed { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public DateTimeOffset? LockoutDate { get; set; }
        public int AccessTries { get; set; } = DefaultAccessTries;
        public UserAccountStatus AccountStatus { get; set; }
        public DateTimeOffset TimeOfUserAgreement { get; set; }
        public string AvatarPath { get; set; }
        public string StripeCustomerId { get; set; }
        public string StripeSubscriptionId { get; set; }
        public bool ProvidedPaymentDetails { get; set; }
        public long? CircleId { get; set; }
        public DateTimeOffset? CircleJoinDate { get; set; }

        // Notification Profile
        public Guid NotificationId { get; set; }
        public bool IssuePosts { get; set; } = DefaultIssuePosts;
        public bool IssueReminders { get; set; } = DefaultIssueReminders;

        // Navigation Properties
        public Circle Circle { get; set; }
        public List<UserReport> ReporterList { get; set; }
        public List<UserReport> ReportedList { get; set; }
        public List<Block> BlockerList { get; set; }
        public List<Block> BlockedList { get; set; }
        public List<PostReport> SnapshotReports { get; set; }
        public List<Subscription> Subscriptions { get; set; }
        public List<Feedback> Feedback { get; set; }
        public List<Notification> Notifications { get; set; }
        public List<Post> Posts { get; set; }
        public List<Recipient> Recipients { get; set; }

        // Default Values
        public static int DefaultAccessTries { get; set; } = 3;

        // Notification Profile
        public static bool DefaultIssuePosts { get; set; } = true;
        public static bool DefaultIssueReminders { get; set; } = true;
        public static bool DefaultGatheringReminders { get; set; } = true;
        public static bool DefaultGatheringActivity { get; set; } = true;
        public static bool DefaultGatheringDiscovery { get; set; } = true;
    }
}

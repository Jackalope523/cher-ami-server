using Repository.Databases.Entities.Messages;
using Repository.Databases.Entities.Reports;
using UserReport = Repository.Databases.Entities.Reports.UserReport;

namespace Repository.Databases.Entities
{
    public class User : Entity
    {
        public string PhoneNumber { get; set; } = DefaultPhoneNumber;
        public string Email { get; set; } = DefaultEmail;
        public string NormalizedEmail { get; set; } = DefaultNormalisedEmail;
        public string Title { get; set; } = DefaultTitle;
        public string FirstName { get; set; } = DefaultFirstName;
        public string LastName { get; set; } = DefaultLastName;
        public DateTimeOffset DateOfBirth { get; init; } = DefaultDateOfBirth;
        public DateTimeOffset JoinDate { get; init; } = DefaultJoinDate;
        public int Reputation { get; set; } = DefaultReputation;
        public bool IsPhoneConfirmed { get; set; } = DefaultIsPhoneConfirmed;
        public bool IsEmailConfirmed { get; set; } = DefaultIsEmailConfirmed;
        public string SecurityStamp { get; set; } = DefaultSecurityStamp;
        public DateTimeOffset? LockoutDate { get; set; } = DefaultLockoutDate;
        public int AccessTries { get; set; } = DefaultAccessTries;
        public UserAccountStatus AccountStatus { get; set; } = DefaultAccountStatus;
        public DateTimeOffset TimeOfUserAgreement { get; set; } = DefaultTimeOfUserAgreement;

        // Notification Profile
        public Guid NotificationId { get; set; }
        public bool SocialInvitations { get; set; } = DefaultSocialInvitations;
        public bool CompanionActivity { get; set; } = DefaultCompanionActivity;
        public bool GatheringReminders { get; set; } = DefaultGatheringReminders;
        public bool GatheringActivity { get; set; } = DefaultGatheringActivity;
        public bool GatheringDiscovery { get; set; } = DefaultGatheringDiscovery;

        // Navigation Properties
        public List<Circle>? HostedGatherings { get; set; }
        public List<UserRelationship>? InitiatedUserRelationships { get; set; }
        public List<UserRelationship>? TargetUserRelationships { get; set; }
        public List<CircleMembership>? GatheringLinks { get; set; }
        public List<UserReport>? ReporterList { get; set; }
        public List<UserReport>? ReporteeList { get; set; }
        public List<SnapshotReport>? SnapshotReports { get; set; }
        public List<Snapshot>? Snapshots { get; set; }
        public List<Subscription>? Subscriptions { get; set; }
        public List<Feedback>? Feedback { get; set; }
        public List<Notification>? Notifications { get; set; }
        public List<ChatMembership>? ChatLinks { get; set; }
        public List<Message>? Messages { get; set; }
        public List<ProfileMessage>? Shares { get; set; }
        public List<Connection>? Connections { get; set; }
        public List<Post>? Posts { get; set; }

        // Default Values
        public static string DefaultPhoneNumber { get; set; } = "";
        public static string DefaultEmail { get; set; } = "";
        public static string DefaultNormalisedEmail { get; set; } = "";
        public static string DefaultTitle { get; set; } = "";
        public static string DefaultFirstName { get; set; } = "";
        public static string DefaultLastName { get; set; } = "";
        public static DateTimeOffset DefaultDateOfBirth { get; set; } = DateTimeOffset.MinValue;
        public static DateTimeOffset DefaultJoinDate { get; set; } = DateTimeOffset.MinValue;
        public static int DefaultReputation { get; set; } = 50;
        public static bool DefaultIsPhoneConfirmed { get; set; } = false;
        public static bool DefaultIsEmailConfirmed { get; set; } = false;
        public static string DefaultSecurityStamp { get; set; } = "";
        public static DateTimeOffset? DefaultLockoutDate { get; set; } = null;
        public static int DefaultAccessTries { get; set; } = 3;
        public static UserAccountStatus DefaultAccountStatus { get; set; } = UserAccountStatus.Active;
        public static DateTimeOffset DefaultTimeOfUserAgreement { get; set; } = DateTimeOffset.MinValue;

        // Notification Profile
        public static bool DefaultSocialInvitations { get; set; } = true;
        public static bool DefaultCompanionActivity { get; set; } = true;
        public static bool DefaultGatheringReminders { get; set; } = true;
        public static bool DefaultGatheringActivity { get; set; } = true;
        public static bool DefaultGatheringDiscovery { get; set; } = true;
    }
}

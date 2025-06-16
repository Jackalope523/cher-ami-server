using NetTopologySuite.Geometries;
using Repository.Entities;

namespace Repository
{
    public class User : Entity
    {
        public string PhoneNumber { get; set; } = DefaultPhoneNumber;
        public string Email { get; set; } = DefaultEmail;
        public string NormalisedEmail { get; set; } = DefaultNormalisedEmail;
        public string Name { get; set; } = DefaultName;
        public string CompanionshipCode { get; set; } = DefaultCompanionshipCode;
        public DateTimeOffset DateOfBirth { get; init; } = DefaultDateOfBirth;
        public DateTimeOffset JoinDate { get; init; } = DefaultJoinDate;
        public int Reputation { get; set; } = DefaultReputation;
        public bool IsPhoneConfirmed { get; set; } = DefaultIsPhoneConfirmed;
        public bool IsEmailConfirmed { get; set; } = DefaultIsEmailConfirmed;
        public string SecurityStamp { get; set; } = DefaultSecurityStamp;
        public DateTimeOffset? LockoutDate { get; set; } = DefaultLockoutDate;
        public int AccessTries { get; set; } = DefaultAccessTries;
        public UserAccountStatus AccountStatus { get; set; } = DefaultAccountStatus;
        public long? CurrentGathering { get; set; } = DefaultCurrentGathering;
        public DateTimeOffset TimeOfUserAgreement { get; set; } = DefaultTimeOfUserAgreement;

        // Vector
        public int Extroversion { get; init; } = DefaultExtroversion;
        public int Athleticisme { get; init; } = DefaultAthleticisme;
        public int Openness { get; init; } = DefaultOpenness;
        public int Chaos { get; init; } = DefaultChaos;
        public int Competitiveness { get; init; } = DefaultCompetitiveness;
        public int Industriousness { get; init; } = DefaultIndustriousness;
        public int NightOwl { get; init; } = DefaultNightOwl;
        public int Age { get; init; } = DefaultAge;

        //Geolocation: X = Longitude Y = Latitude
        public Point Haunt { get; set; } = DefaultHaunt;
        public double HauntRadius { get; set; } = DefaultHauntRadius;
        public int HauntWeight { get; set; } = DefaultHauntWeight;
        public Point CurrentLocation { get; set; } = DefaultCurrentLocation;
        public double CurrentRadius { get; set; } = DefaultCurrentRadius;

        // Notification Profile
        public Guid NotificationId { get; set; }
        public bool SocialInvitations { get; set; } = DefaultSocialInvitations;
        public bool CompanionActivity { get; set; } = DefaultCompanionActivity;
        public bool GatheringReminders { get; set; } = DefaultGatheringReminders;
        public bool GatheringActivity { get; set; } = DefaultGatheringActivity;
        public bool GatheringDiscovery { get; set; } = DefaultGatheringDiscovery;

        // Navigation Properties
        public List<Gathering>? HostedGatherings { get; set; }
        public List<UserRelationship>? InitiatedUserRelationships { get; set; }
        public List<UserRelationship>? TargetUserRelationships { get; set; }
        public List<GatheringLink>? GatheringLinks { get; set; }
        public List<SnapshotLink>? SnapshotLinks { get; set; }
        public List<UserReport>? ReporterList { get; set; }
        public List<UserReport>? ReporteeList { get; set; }
        public List<GatheringReport>? GatheringReports { get; set; }
        public List<SnapshotReport>? SnapshotReports { get; set; }
        public List<Snapshot>? Snapshots { get; set; }
        public List<Telegram>? SentTelegrams { get; set; }
        public List<Telegram>? ReceivedTelegrams { get; set; }
        public List<Subscription>? Subscriptions { get; set; }
        public List<Penalty>? Penalties { get; set; }
        public List<Feedback>? Feedback { get; set; }
        public List<GuestClearance>? GuestClearances { get; set; }
        public List<Notification>? Notifications { get; set; }
        public List<ChatLink>? ChatLinks { get; set; }
        public List<Message>? Messages { get; set; }
        public List<ProfileMessage>? Shares { get; set; }
        public List<Connection>? Connections { get; set; }

        // Default Values
        public static string DefaultPhoneNumber { get; set; } = "";
        public static string DefaultEmail { get; set; } = "";
        public static string DefaultNormalisedEmail { get; set; } = "";
        public static string DefaultName { get; set; } = "";
        public static string DefaultCompanionshipCode { get; set; } = "";
        public static DateTimeOffset DefaultDateOfBirth { get; set; } = DateTimeOffset.MinValue;
        public static DateTimeOffset DefaultJoinDate { get; set; } = DateTimeOffset.MinValue;
        public static int DefaultReputation { get; set; } = 50;
        public static bool DefaultIsPhoneConfirmed { get; set; } = false;
        public static bool DefaultIsEmailConfirmed { get; set; } = false;
        public static string DefaultSecurityStamp { get; set; } = "";
        public static DateTimeOffset? DefaultLockoutDate { get; set; } = null;
        public static int DefaultAccessTries { get; set; } = 3;
        public static UserAccountStatus DefaultAccountStatus { get; set; } = UserAccountStatus.Active;
        public static long? DefaultCurrentGathering { get; set; } = null;
        public static DateTimeOffset DefaultTimeOfUserAgreement { get; set; } = DateTimeOffset.MinValue;

        // Vector
        public static int DefaultExtroversion { get; set; } = 50;
        public static int DefaultAthleticisme { get; set; } = 50;
        public static int DefaultOpenness { get; set; } = 50;
        public static int DefaultChaos { get; set; } = 50;
        public static int DefaultCompetitiveness { get; set; } = 50;
        public static int DefaultIndustriousness { get; set; } = 50;
        public static int DefaultNightOwl { get; set; } = 50;
        public static int DefaultAge { get; set; } = 25;

        private static readonly CoordinateFactory Factory = new();

        // Geolocation: X = Longitude Y = Latitude     
        public static Point DefaultHaunt { get; set; } = Factory.Create(7.540, 53.483);
        public static double DefaultHauntRadius { get; set; } = 10;
        public static int DefaultHauntWeight { get; set; } = 0;
        public static Point DefaultCurrentLocation { get; set; } = Factory.Create(7.544, 53.483);
        public static double DefaultCurrentRadius { get; set; } = 10;

        // Notification Profile
        public static Guid DefaultNotificationId { get; set; } = Guid.Empty;
        public static bool DefaultSocialInvitations { get; set; } = true;
        public static bool DefaultCompanionActivity { get; set; } = true;
        public static bool DefaultGatheringReminders { get; set; } = true;
        public static bool DefaultGatheringActivity { get; set; } = true;
        public static bool DefaultGatheringDiscovery { get; set; } = true;
    }
}

using NetTopologySuite.Geometries;

namespace Repository
{
    public class Gathering : Entity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset TimeOfCreation { get; set; }
        public long? HostId { get; set; }

        // X = Longitude Y = Latitude
        public Point Location { get; set; }
        public string FriendlyLocation { get; set; }

        public GatheringState State { get; set; }
        public GatheringVisibility Visibility { get; set; }
        public int GroupMinimum { get; set; }
        public int GroupMaximum { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public double Radius { get; set; }
        public bool IsDynamic { get; set; }
        public int NumberOfGuests { get; set; }
        public int DegreeOfPrivacy { get; set; }
        public float Decay { get; set; }

        // Vector
        public int Extroversion { get; init; }
        public int Athleticisme { get; init; }
        public int Openness { get; init; }
        public int Chaos { get; init; }
        public int Competitiveness { get; init; }
        public int Industriousness { get; init; }
        public int NightOwl { get; init; }
        public int Age { get; init; }

        // Navigation Properties
        public User? Host { get; set; }
        public GatheringChat? Chat { get; set; }
        public List<GatheringLink>? GatheringLink { get; set; }
        public List<GatheringReport>? GatheringReports { get; set; }
        public List<UserReport>? UserReports { get; set; }
        public List<Snapshot>? Snapshots { get; set; }
        public List<GuestClearance>? GuestClearances { get; set; }
        public List<Notification>? Notifications { get; set; }
        public List<GatheringShareMessage>? Shares { get; set; }
        public List<GatheringInviteMessage>? Invites { get; set; }

        // Default Values
        private static readonly CoordinateFactory Factory = new();

        public static string DefaultHeroImageURL { get; set; } = "";
        public static string DefaultTitle { get; set; } = "Lewis";
        public static string DefaultDescription { get; set; } = "A dog named Lewis.";
        public static DateTimeOffset DefaultStartTime { get; set; } = DateTimeOffset.MinValue;
        public static DateTimeOffset DefaultTimeOfCreation { get; set; } = DateTimeOffset.MinValue;
        public static long? DefaultHostId { get; set; } = null;
        public static Point DefaultLocation { get; set; } = Factory.Create(7.544, 53.483);
        public static string DefaultFriendlyLocation { get; set; } = "Solitude";
        public static GatheringState DefaultState { get; set; } = GatheringState.Alive;
        public static GatheringVisibility DefaultVisibility { get; set; } = GatheringVisibility.Visible;
        public static int DefaultGroupMinimum { get; set; } = 0;
        public static int DefaultGroupMaximum { get; set; } = 10;
        public static double DefaultRadius { get; set; } = 10.000;
        public static bool DefaultIsDynamic { get; set; } = false;
        public static int DefaultNumberOfGuests { get; set; } = 0;
        public static int DefaultDegreeOfPrivacy { get; set; } = 3;
        public static float DefaultDecay { get; set; } = 100;

        // Vector
        public static int DefaultExtroversion { get; set; } = 50;
        public static int DefaultAthleticisme { get; set; } = 50;
        public static int DefaultOpenness { get; set; } = 50;
        public static int DefaultChaos { get; set; } = 50;
        public static int DefaultCompetitiveness { get; set; } = 50;
        public static int DefaultIndustriousness { get; set; } = 50;
        public static int DefaultNightOwl { get; set; } = 50;
        public static int DefaultAge { get; set; } = 25;
    }
}

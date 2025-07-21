using NetTopologySuite.Geometries;
using Repository.Databases.Entities.Chats;
using Repository.Databases.Entities.Messages;
using Repository.Databases.Entities.Reports;
using Repository.Databases.Factories;

namespace Repository.Databases.Entities
{
    public class Circle : Entity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset TimeOfCreation { get; set; }

        public int GroupMinimum { get; set; }
        public int GroupMaximum { get; set; }
        public int NumberOfGuests { get; set; }
        public int DegreeOfPrivacy { get; set; }

        // Navigation Properties
        public CircleChat? Chat { get; set; }
        public List<CircleMembership>? CircleMemberships { get; set; }
        public List<Notification>? Notifications { get; set; }

        // Default Values

        public static string DefaultHeroImageURL { get; set; } = "";
        public static string DefaultTitle { get; set; } = "Lewis";
        public static string DefaultDescription { get; set; } = "A dog named Lewis.";
        public static DateTimeOffset DefaultTimeOfCreation { get; set; } = DateTimeOffset.MinValue;
        public static int DefaultGroupMinimum { get; set; } = 0;
        public static int DefaultGroupMaximum { get; set; } = 10;
        public static int DefaultNumberOfGuests { get; set; } = 0;
        public static int DefaultDegreeOfPrivacy { get; set; } = 3;
    }
}

using NetTopologySuite.Geometries;
using Repository.Databases.Entities.Chats;
using Repository.Databases.Entities.Messages;
using Repository.Databases.Entities.Reports;
using Repository.Databases.Factories;

namespace Repository.Databases.Entities
{
    public class Circle : Entity
    {
        public string Title { get; set; } = DefaultTitle;
        public DateTimeOffset TimeOfCreation { get; set; }
        public string CircleCode { get; set; } = DefaultCircleCode;
        public CirclePlan Plan { get; set; }
        public IssueSchedule IssueSchedule { get; set; }


        // Navigation Properties
        public CircleChat? Chat { get; set; }
        public List<CircleMembership>? CircleMemberships { get; set; }
        public List<Notification>? Notifications { get; set; }

        // Default Values

        public static string DefaultTitle { get; set; } = "";
        public static string DefaultCircleCode { get; set; } = "";
        public static DateTimeOffset DefaultTimeOfCreation { get; set; } = DateTimeOffset.MinValue;
    }
}

using Microsoft.Net.Http.Headers;
using Repository.Databases.Entities.Chats;

namespace Repository.Databases.Entities
{
    public class Circle : Entity
    {
        public string Title { get; set; } = DefaultTitle;
        public DateTimeOffset TimeOfCreation { get; set; }
        public string CircleCode { get; set; } = DefaultCircleCode;
        public CirclePlan Plan { get; set; }
        public IssueSchedule IssueSchedule { get; set; }
        public string HeaderFilename { get; set; } = DefaultHeaderFilename;


        // Navigation Properties
        public List<CircleMembership>? CircleMemberships { get; set; }
        public List<CircleRecipient>? CircleRecipients { get; set; }
        public List<Issue>? Issues { get; set; }
        public List<Notification>? Notifications { get; set; }
        public CircleChat? Chat { get; set; }

        // Default Values

        public static string DefaultTitle { get; set; } = "";
        public static string DefaultCircleCode { get; set; } = "";
        public static string DefaultHeaderFilename { get; set; } = "";
        public static DateTimeOffset DefaultTimeOfCreation { get; set; } = DateTimeOffset.MinValue;
    }
}

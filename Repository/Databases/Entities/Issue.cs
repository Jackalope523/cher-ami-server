namespace Repository.Databases.Entities
{
    public class Issue : Entity
    {
        public enum IssueStatus
        {
            Drafting,
            Published, 
            Shipped, 
            Archived,
        }

        public long CircleId { get; set; }
        public string Title { get; set; } = DefaultTitle;
        public int IssueNumber { get; set; }
        public DateTimeOffset DraftingStart { get; set; }
        public DateTimeOffset DraftingEnd { get; set; }
        public IssueStatus Status { get; set; }
        public IssueType Type { get; set; }
        public string HeaderFilename { get; set; } = DefaultHeaderFilename;

        // Navigation Properties
        public Circle? Circle { get; set; }
        public List<Post>? Posts { get; set; }

        // Default Values
        public static string DefaultTitle { get; set; } = "";
        public static string DefaultHeaderFilename { get; set; } = "";
    }
}

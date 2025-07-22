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
        public DateTimeOffset PublicationDate { get; set; }
        public IssueStatus Status { get; set; }

        // Navigation Properties
        public Circle? Circle { get; set; }

        // Default Values
        public static string DefaultTitle { get; set; } = "";
    }
}

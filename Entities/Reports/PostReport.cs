namespace CherAmiAPI.Entities.Reports
{
    public enum PostReportType
    {
        Embarrassing, 
        Inappropriate,
        GraphicContent, 
        ManipulatedMedia,
        Spam, 
        Other,
    }

    public class PostReport : Report
    {
        public PostReportType Type { get; set; }

        public long PostId { get; init; }

        // Navigation Properties
        public Post Post { get; init; }
    }
}

using System;

using System.ComponentModel.DataAnnotations;

namespace Frontier.Contracts.Requests
{
	public class UserReportManifest
    {
        [Required]
        public UserReportType ReportType { get; set; }

        public string ReportDetails { get; set; }

        public long? CircleId { get; set; }
    }

    public class PostReportManifest
    {
        [Required]
        public PostReportType ReportType { get; set; }

        public string ReportDetails { get; set; }
    }
}


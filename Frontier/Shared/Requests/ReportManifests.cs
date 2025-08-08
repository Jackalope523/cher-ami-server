using System;

using System.ComponentModel.DataAnnotations;

namespace Frontier.Contracts.Requests
{
	

    public class PostReportManifest
    {
        [Required]
        public PostReportType ReportType { get; set; }

        public string ReportDetails { get; set; }
    }
}


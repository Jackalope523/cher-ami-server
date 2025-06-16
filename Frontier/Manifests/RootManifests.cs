using System;

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Core.Boundaries;

namespace Frontier.Manifests
{
	public class CanaryDetailsManifest
    {
        public string MinimumVersion { get; set; }
        public string ServerVersion { get; set; }
        public int PageSize { get; set; }
    }

	public class FeedbackManifest
    {
        [Required]
        public string Comments { get; set; }

        public string Pseudonym { get; set; }
    }
}


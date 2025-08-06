using System.ComponentModel.DataAnnotations;
using LazyLizardBackend.Contracts.Responses;
using Microsoft.AspNetCore.Http;

namespace Frontier.Contracts.Requests
{
	public class CircleCreationManifest
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public CirclePlan Plan { get; set; }

        [Required]
        public IssueSchedule Schedule { get; set; }

        public IFormFile Image { get; set; }
    }

    public class CircleEditManifest
    {
        public string Title { get; set; }

        public CirclePlan Plan { get; set; }

        public IssueSchedule Schedule { get; set; }

        public IFormFile Image { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Core.Boundaries;

using Microsoft.Extensions.Hosting;
using NetTopologySuite.Utilities;
using Microsoft.AspNetCore.Http;

namespace Frontier.Manifests
{
	public class GroupCreationManifest
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public GroupPlan Plan { get; set; }

        [Required]
        public IssueSchedule Schedule { get; set; }

        public IFormFile Image { get; set; }
    }

    public class GroupEditManifest
    {
        public string Title { get; set; }

        public GroupPlan Plan { get; set; }

        public IssueSchedule Schedule { get; set; }

        public IFormFile Image { get; set; }
    }
}

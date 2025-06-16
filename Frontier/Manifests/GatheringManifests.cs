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
	public class GatheringCreationManifest
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public float Latitude { get; set; }

        [Required]
        public float Longitude { get; set; }

        [Required]
        public string FriendlyLocation { get; set; }

        [Required]
        public DateTimeOffset StartTime { get; set; }

        [Required]
        public float Radius { get; set; }

        [Required]
        public bool IsDynamic { get; set; }

        [Required]
        public int DegreeOfPrivacy { get; set; }

        public int? GroupMinimum { get; set; }
        public int? GroupMaximum { get; set; }

        public IFormFile Image { get; set; }

    }
    public class GatheringEditManifest
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public string FriendlyLocation { get; set; }

        public DateTimeOffset? StartTime { get; set; }

        public float? Radius { get; set; }

        public bool? IsDynamic { get; set; }

        public int? DegreeOfPrivacy { get; set; }

        public int? GroupMinimum { get; set; }
        public int? GroupMaximum { get; set; }

        public IFormFile Image { get; set; }

    }

    public class SnapshotManifest
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}

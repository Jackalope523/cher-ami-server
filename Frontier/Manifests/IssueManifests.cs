using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

namespace Frontier.Manifests
{
	public class PostCreationManifest
    {
        [Required]
        public DateTime Time { get; set; }

        public string Caption { get; set; }

        public IFormFile Image { get; set; }
    }

    public class PostEditManifest
    {
        public DateTime Time { get; set; }

        public string Caption { get; set; }

        public IFormFile Image { get; set; }
    }
}

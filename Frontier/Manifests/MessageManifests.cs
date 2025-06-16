using Core.Boundaries;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Manifests
{
	public class GroupChatCreationManifest
    {
        public string Title { get; set; }

        [Required]
        public long[] ParticipantIds { get; set; }
    }

	public class GroupChatEditManifest
    {
        public string Title { get; set; }

        public IFormFile Image { get; set; }
    }
}

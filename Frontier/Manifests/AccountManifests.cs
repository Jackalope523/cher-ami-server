using Core.Boundaries;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Manifests
{
	public class TargetManifest
    {
        [Required]
        public long TargetId { get; set; }
    }

    public class AccountCredentialsManifest
    {
		[Required]
		public string PhoneNumber { get; set; }

		public string Code { get; set; }

		public bool? UseWhatsApp { get; set; }
    }

    public class AccountSignUpManifest
	{
		[Required]
		public string PhoneNumber { get; set; }

		public string Email { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public DateTime DateOfBirth { get; set; }
    }

	public class ImageManifest
	{
		[Required]
		public IFormFile Image { get; set; }
	}
}

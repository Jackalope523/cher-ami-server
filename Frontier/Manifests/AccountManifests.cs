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


		public string Title { get; set; }

		[Required]
		public string GivenName { get; set; }

		[Required]
		public string FamilyName { get; set; }

		[Required]
		public DateTime DateOfBirth { get; set; }

		public string CircleCode { get; set; }
    }

    public class AccountEditManifest
    {
        public string Email { get; set; }

        public string Title { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
		public DateTime DateOfBirth { get; set; }
    }
}

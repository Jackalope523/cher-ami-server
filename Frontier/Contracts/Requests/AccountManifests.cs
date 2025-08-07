using Core.Boundaries;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Frontier.Contracts.Requests
{
	public class TargetManifest
    {
        [Required]
        public long TargetId { get; set; }
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

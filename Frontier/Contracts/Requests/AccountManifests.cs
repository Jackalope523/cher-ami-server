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

   

   

    
}

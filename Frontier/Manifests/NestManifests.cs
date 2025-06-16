using System;

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Core.Boundaries;

namespace Frontier.Manifests
{
	public class AccountEditManifest
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class AccountRatingManifest
    {
        [Required]
        public SnapshotAcclaim Action { get; set; }
    }
}


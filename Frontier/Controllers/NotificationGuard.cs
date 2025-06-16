using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using Core.Boundaries;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Frontier.Controllers
{
	[Route("notifications")]
	public class NotificationGuard : AbstractGuard
	{
		#region Initialisation

		public NotificationGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet]
		public async Task<IActionResult> GetNotificationPreferences()
		{
			return await Execute(async user =>
			{
				return await notifications.GetNotificationPreferencesAsync(user.Id);
			});
        }

		[HttpPost]
		public async Task<IActionResult> UpdateNotificationPreferences(
			bool? social_invitations = null, bool? companion_activity = null,
			bool? gathering_reminders = null, bool? gathering_activity = null,
			bool? gathering_discovery = null)
		{
			return await Execute(async user =>
			{
				await notifications.UpdateNotificationPreferencesAsync(user.Id,
					socialInvitations: social_invitations, companionActivity: companion_activity,
					gatheringReminders: gathering_reminders, gatheringActivity: gathering_activity,
					gatheringDiscovery: gathering_discovery);
			});
        }

		#endregion
	}
}
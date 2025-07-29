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
	public class NotificationController : AbstractController
	{
		#region Initialisation

		public NotificationController(ControllerBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet]
		public async Task<IActionResult> GetNotificationPreferences()
		{
			return await Execute(async user =>
				await notifications.GetNotificationPreferencesAsync(user.Id)
			);
        }

		[HttpPost]
		public async Task<IActionResult> UpdateNotificationPreferences(
			bool? issue_posts = null, bool? issue_reminders = null)
		{
			return await Execute(async user =>
				await notifications.UpdateNotificationPreferencesAsync(user.Id,
					issuePosts: issue_posts, issueReminders: issue_reminders)
			);
        }

		#endregion
	}
}
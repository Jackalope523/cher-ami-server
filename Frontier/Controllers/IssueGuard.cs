using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using Core.Boundaries;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Frontier.Controllers
{
    [Route("issue")]
    public class IssueGuard : AbstractGuard
    {
		#region Initialisation

		public IssueGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet("{issueId}")]
        public async Task<IActionResult> GetIssue(long issueId)
        {
			return await Execute(async user =>
				await issues.GetIssueAsync(user.Id, issueId)
            );
        }

		[HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupIssues(long groupId)
        {
			// Verify parameters
            if (!ModelState.IsValid)
            { return MissingInformation(); }

			return await Execute(async user =>
				await issues.GetIssuesForGroupAsync(user.Id, groupId)
            );
        }

        [HttpGet("{issueId}/posts")]
        public async Task<IActionResult> GetGallery(long issueId)
        {
            return await Execute(async user =>
                await issues.GetPostsForIssueAsync(user.Id, issueId)
            );
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetPost(long postId)
        {
            return await Execute(async user =>
                await issues.GetPostAsync(user.Id, postId)
            );
        }

        [HttpPost("{issueId}/posts")]
        public async Task<IActionResult> AddPost(long issueId, [FromForm] PostCreationManifest post)
        {
            // Verify parameters
            if (post == null || !ModelState.IsValid ||
                post.Image == null || post.Image.Length == 0)
            { return MissingInformation(); }

            return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                await post.Image.CopyToAsync(stream);

                return await issues.AddPostAsync(user.Id, issueId, post.Time, post.Caption, stream);
            });
        }

        [HttpPost("post/{postId}")]
        public async Task<IActionResult> EditPost(long postId, [FromForm] PostEditManifest post)
        {
            // Verify parameters
            if (post == null)
            { return MissingInformation(); }

            return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                if (post.Image != null && post.Image.Length > 0)
                {
                    await post.Image.CopyToAsync(stream);
                }

                return await issues.AddPostAsync(user.Id, postId,
                    timestamp: post.Time,
                    caption: post.Caption,
                    image: stream);
            });
        }

        [HttpDelete("post/{postId}")]
        public async Task<IActionResult> RemovePost(long issueId, long postId)
        {
            return await Execute(async user =>
                await issues.DeletePostAsync(user.Id, postId)
            );
        }

		[HttpGet("post/{postId}/report")]
        public async Task<IActionResult> AvailablePostReports(long issueId, long postId)
        {
            return await Execute(async user =>
                await reports.GetAvailableReportsForPostAsync(user.Id, postId)
            );
        }

        [HttpPost("post/{postId}/report")]
        public async Task<IActionResult> ReportPost(long issueId, long postId, [FromBody] PostReportManifest report)
        {
            // Verify parameters
            if (report == null || !ModelState.IsValid)
            { return MissingInformation(); }

            return await Execute(async user =>
                await reports.ReportPostAsync(user.Id, postId, report.ReportType, report.ReportDetails)
            );
        }

        #endregion
    }
}
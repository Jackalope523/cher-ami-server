using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using static Core.Entities.Arbiter;
using static Core.Entities.Psijic;

namespace Core.Controls
{
    internal class ReportDirector : AbstractDirector, IReportOperations
	{
		#region Initialisation

		public ReportDirector(CoreTerminal terminal) : base(terminal) { }

        #endregion

        #region Operations

        public async Task<List<UserReportType>> GetAvailableReportsForUserAsync(long userId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var targetUser = await GetUserAsync(targetId);

            // Verify user can report
            Verify(await user.CanReport(),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_COOLDOWN));

            // Gather recent reports by user against target 
            return await user.AvailableReportTypes(targetUser);
        }

        public async Task ReportUserAsync(long userId, long targetId,
            UserReportType reportType, string reportDetails,
            long? gatheringId = null)
        {
            var user = await GetUserAsync(userId);
            var targetUser = await GetUserAsync(targetId);

            // Verify user can report
            Verify(await user.CanReport(),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_COOLDOWN));

            // Prevent double reports
            Verify(await user.CanReport(targetUser, reportType),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_DUPLICATE));

            // Check if gathering id was supplied
            if (gatheringId.HasValue)
            {
                // Validate both users were at the gathering
                var occuringGathering = await GetCircleAsync(gatheringId.Value);

                bool mutualGuestship = await occuringGathering.HasOnGuestList(user) &&
                    await occuringGathering.HasOnGuestList(targetUser);

                if (mutualGuestship)
                {
                    await Reports.ReportUserAsync(userId, occuringGathering.Id, targetUser.Id, Time, reportType, reportDetails);
                }
                else
                {
                    // Silently drop if mutual guestship not established
                    await Reports.ReportUserAsync(userId, targetUser.Id, Time, reportType, reportDetails);
                }
            }
            else
            {
                await Reports.ReportUserAsync(userId, targetUser.Id, Time, reportType, reportDetails);
            }

            // Compute user's standing
            var status = await targetUser.Reported();

            // Check if user should be punished
            if (targetUser.AccountStatus != status)
            {
                _ = Accounts.UpdateUserAsync(targetUser.Id, new() { (nameof(CoreUser.AccountStatus), status) });
            }
        }

        public async Task<List<PostReportType>> GetAvailableReportsForPostAsync(long userId, long postId)
        {
            var user = await GetUserAsync(userId);
            var targetPost = await Issues.GetPostAsync(postId);
            User targetUser = await GetUserAsync(targetPost.UserId);

            // Verify user can report
            Verify(await user.CanReport(),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_COOLDOWN));

            // Gather recent reports by user against target 
            return await user.AvailableReportTypes(targetPost, targetUser);
        }

        public async Task ReportPostAsync(long userId, long snapshotId,
            PostReportType reportType, string reportDetails)
        {
            var user = await GetUserAsync(userId);
            var targetPost = await Issues.GetPostAsync(snapshotId);
            User targetUser = await GetUserAsync(targetPost.UserId);

            // Verify user can report
            Verify(await user.CanReport(),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_COOLDOWN));

            // Prevent double reports
            Verify(await user.CanReport(targetPost, targetUser, reportType),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_DUPLICATE));

            await Reports.ReportPostAsync(user.Id, targetPost.Id, Time, reportType, reportDetails);

            // Compute user's standing
            var status = await targetUser.Reported();

            // Check if user should be punished
            if (targetUser.AccountStatus != status)
            {
                _ = Accounts.UpdateUserAsync(targetUser.Id, new() { (nameof(CoreUser.AccountStatus), status) });
            }
        }

		#endregion

		#region Favours

		internal async Task<(List<UserReport> UserReports, List<PostReport> PostReports)>
            RequestAllReportsAsync(User user)
            => await Reports.GetReportsForUserAsync(user.Id);

        internal async Task<List<PostReport>> RequestPostReportsAsync(long postId)
            => await Reports.GetReportsForPostAsync(postId);

		#endregion
	}
}


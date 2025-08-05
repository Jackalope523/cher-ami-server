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
    public class ReportService : AbstractService, IReportOperations
	{
		#region Initialisation

		public ReportService(CoreTerminal terminal) : base(terminal) { }

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

        public async Task ReportUserAsync(long userId, long targetId, UserReportType reportType, string reportDetails, long? gatheringId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<List<PostReportType>> GetAvailableReportsForPostAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }

        public async Task ReportPostAsync(long userId, long snapshotId, PostReportType reportType, string reportDetails)
        {
            throw new NotImplementedException();
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


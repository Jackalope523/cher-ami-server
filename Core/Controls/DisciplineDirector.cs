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
    internal class DisciplineDirector : AbstractDirector, IDisciplineOperations
	{
		#region Initialisation

		public DisciplineDirector(CoreTerminal terminal) : base(terminal) { }

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
                var occuringGathering = await GetGatheringAsync(gatheringId.Value);

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

        public async Task<List<GatheringReportType>> GetAvailableReportsForGatheringAsync(long userId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
            var gathering = await GetGatheringAsync(gatheringId);

            // Verify user can report
            Verify(await user.CanReport(),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_COOLDOWN));

            // Gather recent reports by user against target 
            return await user.AvailableReportTypes(gathering);
        }

        public async Task ReportGatheringAsync(long userId, long gatheringId,
            GatheringReportType reportType, string reportDetails)
        {
            var user = await GetUserAsync(userId);
            var gathering = await GetGatheringAsync(gatheringId);

            // Verify user can report
            Verify(await user.CanReport(),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_COOLDOWN));

            // Prevent double reports
            Verify(await user.CanReport(gathering, reportType),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_DUPLICATE));

            await Reports.ReportGatheringAsync(user.Id, gathering.Id, Time, reportType, reportDetails);

            // Check if action is to be taken
            if (await gathering.Reported())
            {
                var host = await GetUserAsync(gathering.HostId);

                // Threshold hit, seal gathering
                await Terminal.GatheringDatabase.UpdateGatheringAsync(gathering.Id, new() { (nameof(CoreGathering.Visibility), GatheringVisibility.Sealed) });

                await gathering.NotifyGuests(CanaryNotification.GatheringSealed(await gathering.ToGatheringShard()));

                // Compute host's standing
                var status = await host.GatheringReported();

                // Check if host should be punished
                if (host.AccountStatus != status)
                {
                    _ = Accounts.UpdateUserAsync(host.Id, new() { (nameof(CoreUser.AccountStatus), status) });
                }
            }
        }

        public async Task<List<SnapshotReportType>> GetAvailableReportsForSnapshotAsync(long userId, long snapshotId)
        {
            var user = await GetUserAsync(userId);
            var targetSnapshot = await Snapshots.GetSnapshotAsync(snapshotId);
            User targetUser = await GetUserAsync(targetSnapshot.User.Id);

            // Verify user can report
            Verify(await user.CanReport(),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_COOLDOWN));

            // Gather recent reports by user against target 
            return await user.AvailableReportTypes(targetSnapshot, targetUser);
        }

        public async Task ReportSnapshotAsync(long userId, long snapshotId,
            SnapshotReportType reportType, string reportDetails)
        {
            var user = await GetUserAsync(userId);
            var targetSnapshot = await Snapshots.GetSnapshotAsync(snapshotId);
            User targetUser = await GetUserAsync(targetSnapshot.User.Id);

            // Verify user can report
            Verify(await user.CanReport(),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_COOLDOWN));

            // Prevent double reports
            Verify(await user.CanReport(targetSnapshot, targetUser, reportType),
                new UserErrorException(UserErrorCode.CANNOT_REPORT_DUPLICATE));

            await Reports.ReportSnapshotAsync(user.Id, targetSnapshot.Id, Time, reportType, reportDetails);

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

        internal async Task<List<PenaltyShard>> RequestPenaltiesForUserAsync(User user)
            => await Reports.GetPenaltiesForUserAsync(user.Id);

        internal async Task PenaliseUserAsync(User user, PenaltyType offense, DateTimeOffset timeOfPenalty)
            => await Reports.PenaliseUserAsync(user.Id, offense, timeOfPenalty);

		internal async Task<(List<UserReport> UserReports, List<GatheringReport> GatheringReports, List<SnapshotReport> SnapshotReports)>
            RequestAllReportsAsync(User user)
            => await Reports.GetReportsForUserAsync(user.Id);

        internal async Task<List<GatheringReport>> RequestGatheringReportsAsync(Gathering gathering)
            => await Reports.GetReportsForGatheringAsync(gathering.Id);

		#endregion
	}
}


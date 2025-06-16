using Core.Boundaries;
using Core.Controls;
using Core.Entities;

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests.Controls
{
    public class DisciplineDirectorTests : CoreTest
    {
		private DisciplineDirector director;

        public DisciplineDirectorTests()
        {
			director = environment.Terminal.DisciplineDirector;
        }

		[Fact]
		public async Task ReportUserAsync_ValidUser_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var reportedUser = await environment.GenerateUniqueUserAsync();
			UserReportType report = UserReportType.Rude;
			string reportDetails = "detailed report";

			// Act
			await director.ReportUserAsync(user.Id, reportedUser.Id, report, reportDetails);

			// Assert
			Assert.Single(await reportedUser.Reports);
			Assert.Equal(report, (await reportedUser.Reports)[0].ReportType);
			Assert.Equal(reportDetails, (await reportedUser.Reports)[0].ReportDetails);
		}

		[Fact]
		public async Task ReportUserAsync_MultipleReports_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var reportedUser = await environment.GenerateUniqueUserAsync();
			UserReportType report = UserReportType.Rude;
			string reportDetails = "detailed report";

			// Act
			await director.ReportUserAsync(user.Id, reportedUser.Id, report, reportDetails);
			await director.ReportUserAsync(user.Id, reportedUser.Id, report, reportDetails);
			await director.ReportUserAsync(user.Id, reportedUser.Id, report, reportDetails);

			// Assert
			Assert.Equal(3, (await reportedUser.Reports).Count);
		}

		[Fact]
		public async Task ReportGatheringAsync_ValidGathering_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var reportedHost = await environment.GenerateUniqueUserAsync();
			var reportedGathering = await environment.GenerateUpcomingGatheringAsync(reportedHost);
			GatheringReportType report = GatheringReportType.Misleading;
			string reportDetails = "detailed report";

			// Act
			await director.ReportGatheringAsync(user.Id, reportedGathering.Id, report, reportDetails);

			// Assert
			Assert.Single(await reportedGathering.GatheringReports);
			Assert.Equal(report, (await reportedGathering.GatheringReports)[0].ReportType);
			Assert.Equal(reportDetails, (await reportedGathering.GatheringReports)[0].ReportDetails);

			Assert.Single(await reportedHost.GatheringReports);
			Assert.Equal(report, (await reportedHost.GatheringReports)[0].ReportType);
			Assert.Equal(reportDetails, (await reportedHost.GatheringReports)[0].ReportDetails);
		}

		[Fact]
		public async Task PenaliseUserAsync_ValidUser_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			PenaltyType penalty = PenaltyType.Unreliable;
			DateTimeOffset timeOfPenalty = new DateTime(0);

			// Act
			await director.PenaliseUserAsync(user, penalty, timeOfPenalty);

			// Assert
			Assert.Single(await user.Penalties);
			Assert.Equal(penalty, (await user.Penalties)[0].Offense);
			Assert.Equal(timeOfPenalty, (await user.Penalties)[0].TimeOfPenalty);
		}
	}
}

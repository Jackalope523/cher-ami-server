using Core.Boundaries;
using Frontier.Controllers;
using Frontier.Manifests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

using Xunit;

namespace Frontier.Tests
{
	public class AbstractGuardTests
	{
		static ILogger log = LoggerFactory.Create((ILoggingBuilder obj) => NoOp()).CreateLogger("testing");
		static GuardBox testBox = new(
			new() { Flag = Core.EnvironmentFlag.Development },
			log, null, null, null, null, null, null, null, null, null, null, null);

		AbstractGuard testGuard = new(testBox, null);

		[Fact]
		public async Task Execute_NoData_Success()
		{
			// Arrange
			var action = async () => NoOp();

			// Act
			var result = await testGuard.Execute(action) as ObjectResult;

			// Assert
			Assert.NotNull(result);
			Assert.Null(result.Value);
		}

		[Fact]
		public async Task Execute_Shard_Success()
		{
			// Arrange
			var outgoing = new UserShard(117, "John");
			Func<Task<object>> action = async () => outgoing;

			// Act
			var result = await testGuard.Execute(action) as ObjectResult;

			// Assert
			Assert.NotNull(result);

			var resultManifest = result.Value as UserShard;
			Assert.NotNull(resultManifest);
			Assert.Equal(outgoing.Id, resultManifest.Id);
			Assert.Equal(outgoing.Name, resultManifest.Name);
		}

		[Fact]
		public async Task Execute_List_Success()
		{
			// Arrange
			List<UserShard> outgoing = new() { new UserShard(117, "John"), new UserShard(3, "Thel") };
			Func<Task<object>> action = async () => outgoing;

			// Act
			var result = await testGuard.Execute(action) as ObjectResult;

			// Assert
			Assert.NotNull(result);

			var resultList = result.Value as List<UserShard>;
			Assert.NotNull(resultList);
			Assert.Equal(outgoing.Count, resultList.Count);
		}

		[Fact]
		public async Task Execute_ProtectedData_Failure()
		{
			// Arrange
			CoreUser bastardData = new(117, "John", "", "", "", DateTimeOffset.UtcNow,
				true, true, false, "", null,
				0, UserAccountStatus.Impotent, DateTimeOffset.UtcNow, 0, null, DateTimeOffset.UtcNow,
				Guid.NewGuid());

			Func<Task<object>> action = async () => bastardData;

			// Act
			var result = await testGuard.Execute(action) as IStatusCodeActionResult;

			// Assert
			Assert.NotNull(result);
			Assert.NotEqual(200, result.StatusCode);
		}

		private static void NoOp()
		{ }
	}
}
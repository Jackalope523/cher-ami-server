using System;
using System.Threading.Tasks;
using Core.Entities;

using Xunit;

namespace Core.Tests.Entities
{
	public class ArbiterTests
	{
		[Fact]
		public void Try_Success_DoesNotThrowException()
		{
			// Act & Assert
			Arbiter.Verify(true, new UnexpectedFailureException("")); // No exception should be thrown
		}

		[Fact]
		public void Try_Failure_ThrowsException()
		{
			// Act & Assert
			Assert.Throws<UnexpectedFailureException>(() => Arbiter.Verify(false, new UnexpectedFailureException("")));
		}

		[Fact]
		public void Fail_Failure_DoesNotThrowException()
		{
			// Act & Assert
			Arbiter.FailIf(false, new UnexpectedFailureException("")); // No exception should be thrown
		}

		[Fact]
		public void Fail_Success_ThrowsException()
		{
			// Act & Assert
			Assert.Throws<UnexpectedFailureException>(() => Arbiter.FailIf(true, new UnexpectedFailureException("")));
		}
	}

	public class ArtificerTests
	{
		[Fact]
		public void IsNull_Null_ReturnsTrue()
		{
			// Arrange
			int? value = null;

			// Act
			bool result = Artificer.IsNull(value);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void IsNull_NotNull_ReturnsFalse()
		{
			// Arrange
			int? value = 42;

			// Act
			bool result = Artificer.IsNull(value);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void IsNotNull_Null_ReturnsFalse()
		{
			// Arrange
			int? value = null;

			// Act
			bool result = Artificer.IsNotNull(value);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void IsNotNull_NotNull_ReturnsTrue()
		{
			// Arrange
			int? value = 42;

			// Act
			bool result = Artificer.IsNotNull(value);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void AreNull_AllNull_ReturnsTrue()
		{
			// Arrange
			int? value1 = null;
			int? value2 = null;

			// Act
			bool result = Artificer.AreNull(value1, value2);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void AreNull_NotAllNull_ReturnsFalse()
		{
			// Arrange
			int? value1 = null;
			int? value2 = 42;

			// Act
			bool result = Artificer.AreNull(value1, value2);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void AreNotNull_AllNotNull_ReturnsTrue()
		{
			// Arrange
			int? value1 = 42;
			int? value2 = 100;

			// Act
			bool result = Artificer.AreNotNull(value1, value2);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void AreNotNull_NotAllNotNull_ReturnsFalse()
		{
			// Arrange
			int? value1 = 42;
			int? value2 = null;

			// Act
			bool result = Artificer.AreNotNull(value1, value2);

			// Assert
			Assert.False(result);
		}
	}

	public class PsijicTests
	{
		[Fact]
		public void HappenedBefore_FirstTimeBeforeSecondTime_ReturnsTrue()
		{
			// Arrange
			DateTimeOffset time1 = DateTimeOffset.UtcNow;
			DateTimeOffset time2 = time1.AddMinutes(1);

			// Act
			bool result = Psijic.HappenedBefore(time1, time2);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void HappenedBefore_FirstTimeAfterSecondTime_ReturnsFalse()
		{
			// Arrange
			DateTimeOffset time1 = DateTimeOffset.UtcNow;
			DateTimeOffset time2 = time1.AddMinutes(-1);

			// Act
			bool result = Psijic.HappenedBefore(time1, time2);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void After_FirstTimeAfterSecondTime_ReturnsTrue()
		{
			// Arrange
			DateTimeOffset time1 = DateTimeOffset.UtcNow;
			DateTimeOffset time2 = time1.AddMinutes(-1);

			// Act
			bool result = Psijic.After(time1, time2);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void After_FirstTimeBeforeSecondTime_ReturnsFalse()
		{
			// Arrange
			DateTimeOffset time1 = DateTimeOffset.UtcNow;
			DateTimeOffset time2 = time1.AddMinutes(1);

			// Act
			bool result = Psijic.After(time1, time2);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void HasYet_TimeBeforeCurrentTime_ReturnsFalse()
		{
			// Arrange
			DateTimeOffset pastTime = DateTimeOffset.UtcNow.AddMinutes(-1);

			// Act
			bool result = Psijic.HasYet(pastTime);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void HasYet_TimeAfterCurrentTime_ReturnsTrue()
		{
			// Arrange
			DateTimeOffset futureTime = DateTimeOffset.UtcNow.AddMinutes(1);

			// Act
			bool result = Psijic.HasYet(futureTime);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void HasAlready_TimeBeforeCurrentTime_ReturnsTrue()
		{
			// Arrange
			DateTimeOffset pastTime = DateTimeOffset.UtcNow.AddMinutes(-1);

			// Act
			bool result = Psijic.HasAlready(pastTime);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void HasAlready_TimeAfterCurrentTime_ReturnsFalse()
		{
			// Arrange
			DateTimeOffset futureTime = DateTimeOffset.UtcNow.AddMinutes(1);

			// Act
			bool result = Psijic.HasAlready(futureTime);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void IsWithin_TimeLessThanTime2_ReturnsTrue()
		{
			// Arrange
			TimeSpan time = TimeSpan.FromMinutes(5);
			TimeSpan time2 = TimeSpan.FromMinutes(10);

			// Act
			bool result = Psijic.IsWithin(time, time2);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void IsWithin_TimeGreaterThanTime2_ReturnsFalse()
		{
			// Arrange
			TimeSpan time = TimeSpan.FromMinutes(15);
			TimeSpan time2 = TimeSpan.FromMinutes(10);

			// Act
			bool result = Psijic.IsWithin(time, time2);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task Once_AwaitedTasks_CompletesSuccessfully()
		{
			// Arrange
			Task task1 = Task.Delay(100);
			Task task2 = Task.Delay(200);

			// Act & Assert
			await Psijic.Once(task1, task2);
			// If no exception is thrown, the test is successful
		}
	}
}

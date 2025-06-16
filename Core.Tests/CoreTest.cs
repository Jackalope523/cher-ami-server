using Core.Boundaries;
using Core.Controls;
using Core.Entities;

using System;
using System.Threading.Tasks;
using Xunit;
using System.Threading;

namespace Core.Tests
{
	[Collection("Core Collection")]
	public class CoreTest : IAsyncLifetime, IDisposable
	{
		private static int testNumber = 0;
		
		protected readonly CoreEnvironment environment;

		public CoreTest()
		{
			environment = new(Interlocked.Increment(ref testNumber));
		}

		public async Task InitializeAsync()
		{ }

		public async Task DisposeAsync()
		{ }

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			environment.DisposeEnvironment();
		}
	}
}

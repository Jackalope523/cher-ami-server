using Core.Boundaries;
using Core.Controls;
using Core.Entities;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests.Controls
{
    public class NotificationDirectorTests : CoreTest
    {
		private NotificationDirector director;

        public NotificationDirectorTests()
        {
			director = environment.Terminal.NotificationDirector;
        }

	}
}

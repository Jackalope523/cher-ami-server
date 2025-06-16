using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using Core.Boundaries;
using Frontier.Controllers;

namespace Frontier.Services
{
	public class SocketConnection : ISocketService
    {
        private static ILogger log;
        private static IHubContext<HollowHub, IClientSocket> hub;

        public static void Initialise(ILogger logger, IHubContext<HollowHub, IClientSocket> hubContext)
        {
            log = logger;
            hub = hubContext;
        }

        public async Task BroadcastAsync(Func<IClientSocket, Task> operation, params string[] connectionIds)
        {
            try
            {
                await operation(hub.Clients.Clients(connectionIds));
            }
            catch (Exception ex)
            {
                log.LogError("Error broadcasting {op} to {con}. {e}", nameof(operation), connectionIds, ex);
            }
        }
	}
}


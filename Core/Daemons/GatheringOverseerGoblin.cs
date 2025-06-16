using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using Microsoft.Extensions.Hosting;

using static Core.Entities.Psijic;

namespace Core.Daemons
{
	public class GatheringOverseerGoblin : BackgroundService
    {
        private readonly TimeSpan interval = TimeSpan.FromMinutes(1);

        private CoreTerminal terminal;

        private ILogger log;

        public GatheringOverseerGoblin(CoreTerminal coreTerminal)
        {
            terminal = coreTerminal;

            log = terminal.Log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                log.LogInformation("{goblin} clocking in at {time}.", nameof(GatheringOverseerGoblin), Time);

                try
                {
                    await CorrodeGatheringsAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    log.LogError("{goblin} had trouble: {error}", nameof(GatheringOverseerGoblin), e);
                }

                log.LogInformation("{goblin} clocking out at {time}.", nameof(GatheringOverseerGoblin), Time);

                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task CorrodeGatheringsAsync(CancellationToken stoppingToken)
        {
            var activeGatherings = await terminal.AdminDatabase.GetAllActiveGatheringsAsync(Psijic.Time);

            foreach (var coreGathering in activeGatherings)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                Gathering gathering = new(coreGathering);

                float goblinFrequency = interval.Minutes / 60f;
                float decayPerHour = 100;

                float newDecay = gathering.Decay - decayPerHour * goblinFrequency;

                try
                {
                    // Apply decay if not expired
                    if (newDecay > 0)
                    {
                        await terminal.GatheringDatabase.UpdateGatheringAsync(gathering.Id, new() { (nameof(CoreGathering.Decay), newDecay) });
                    }
                    else
                    {
                        // Terminate gathering
                        await terminal.GatheringDirector.TerminateGatheringAsync(gathering.HostId, gathering.Id);
                    }
                }
                catch (Exception e)
                {
                    log.LogError("{goblin} had trouble corroding {title}: {error}", nameof(GatheringOverseerGoblin), coreGathering.Title, e);
                }
            }
        }

        private async Task<User> GetUserAsync(long userId)
        {
            return new(await terminal.AccountDatabase.FindUserByIdAsync(userId));
        }
    }
}


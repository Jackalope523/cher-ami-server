using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Frontier.Controllers
{
    public partial class HollowHub : Hub<IClientSocket>
    {
        public async Task UserRead(long conversationId)
        {
            var user = await GetCurrentUserAsync();

            await messages.UserReadAsync(user.Id, conversationId);
        }

        public async Task UserComposing(long conversationId, bool isComposing)
        {
            var user = await GetCurrentUserAsync();

            await messages.UserComposingAsync(user.Id, conversationId, isComposing);
        }

        public async Task<MessageShard> SendText(long conversationId, string text)
        {
            var user = await GetCurrentUserAsync();

            return await messages.SendTextAsync(user.Id, conversationId, text);
        }

        public async Task<MessageShard[]> ShareGathering(long conversationId, long[] gatheringIds)
        {
            var user = await GetCurrentUserAsync();

            return await messages.ShareGatheringAsync(user.Id, conversationId, gatheringIds);
        }

        public async Task<MessageShard[]> ShareSnapshot(long conversationId, long[] snapshotIds)
        {
            var user = await GetCurrentUserAsync();

            return await messages.ShareSnapshotAsync(user.Id, conversationId, snapshotIds);
        }

        public async Task<MessageShard[]> ShareNest(long conversationId, long[] nestIds)
        {
            var user = await GetCurrentUserAsync();

            return await messages.ShareNestAsync(user.Id, conversationId, nestIds);
        }
    }
}

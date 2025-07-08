using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Frontier.Controllers
{
    public partial class SocketHub : Hub<IClientSocket>
    {
        public async Task UserRead(long chatId)
        {
            var user = await GetCurrentUserAsync();

            await chats.UserReadAsync(user.Id, chatId);
        }

        public async Task UserComposing(long chatId, bool isComposing)
        {
            var user = await GetCurrentUserAsync();

            await chats.UserComposingAsync(user.Id, chatId, isComposing);
        }

        public async Task<MessageShard> SendText(long chatId, string text)
        {
            var user = await GetCurrentUserAsync();

            return await chats.SendTextAsync(user.Id, chatId, text);
        }

        public async Task<MessageShard[]> ShareIssue(long chatId, long[] circleIds)
        {
            var user = await GetCurrentUserAsync();

            return await chats.ShareIssueAsync(user.Id, chatId, circleIds);
        }

        public async Task<MessageShard[]> SharePost(long chatId, long[] postIds)
        {
            var user = await GetCurrentUserAsync();

            return await chats.SharePostAsync(user.Id, chatId, postIds);
        }

        public async Task<MessageShard[]> ShareProfile(long chatId, long[] profileIds)
        {
            var user = await GetCurrentUserAsync();

            return await chats.ShareProfileAsync(user.Id, chatId, profileIds);
        }
    }
}

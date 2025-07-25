using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Repository.Entities;
using Repository.Entities.Chats;
using Repository.Entities.Messages;

namespace Repository.Repositories
{
    class ChatRepository : Repository, IChatDatabase
    {
        private int pageSize = 10;

        internal ChatRepository(Func<CardinalContext> contextFactory) : base(contextFactory)
        {
        }


        public async Task<MessageShard> AddMessageAsync(long chatId, long userId, DateTimeOffset timestamp, MessageType type, object value)
        {
            Message toAdd;

            switch (type)
            {
                case MessageType.Text:
                    toAdd = new TextMessage()
                    {
                        ChatId = chatId,
                        UserId = userId,
                        Timestamp = timestamp,
                        Text = (string)value,
                    };
                    break;
                case MessageType.Photo:
                    toAdd = new PhotoMessage()
                    {
                        ChatId = chatId,
                        UserId = userId,
                        Timestamp = timestamp,
                    };
                    break;
                case MessageType.Issue:
                    toAdd = new IssueMessage()
                    {
                        ChatId = chatId,
                        UserId = userId,
                        Timestamp = timestamp,
                        GatheringId = (long)value
                    };
                    break;
                case MessageType.Post:
                    toAdd = new SnapshotMessage()
                    {
                        ChatId = chatId,
                        UserId = userId,
                        Timestamp = timestamp,
                        SnapshotId = (long)value
                    };
                    break;
                case MessageType.Profile:
                    toAdd = new ProfileMessage()
                    {
                        ChatId = chatId,
                        UserId = userId,
                        Timestamp = timestamp,
                        ProfileId = (long)value
                    };
                    break;
                case MessageType.Activity:
                    ActivityMessageShard activityMessageShard = (ActivityMessageShard)value;
                    toAdd = new ActivityMessage()
                    {
                        ChatId = chatId,
                        UserId = userId,
                        Timestamp = timestamp,
                        ActivityType = activityMessageShard.Activity,
                        ActorId = activityMessageShard.ActorId,
                        TargetId = activityMessageShard.TargetId,
                        Text = activityMessageShard.Info,
                    };
                    break;
                default:
                    throw new InvalidInputException("Message of type \"" + type.ToString() + "\" is not supported in this method.");
            }

            await using CardinalContext ctx = initContext();
            ctx.Messages.Add(toAdd);
            await ctx.SaveChangesAsync();

            return new MessageShard(toAdd.Id, toAdd.UserId ?? 0, toAdd.Timestamp, type, value);
        }

        public async Task AddUsersToChatAsync(long chatId, params long[] userIds)
        {
            await using CardinalContext ctx = initContext();

            foreach (long userId in userIds)
            {
                ChatMembership toAdd = new()
                {
                    ChatId = chatId,
                    UserId = userId,
                    LastSeen = DateTime.UtcNow,
                    Type = ChatMembershipType.Regular
                };

                ctx.ChatMemberships.Add(toAdd);
            }

            await ctx.SaveChangesAsync();
        }

        public async Task DeleteChatAsync(long chatId)
        {
            await using CardinalContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Messages.Where(m => m.ChatId == chatId).ExecuteDeleteAsync();
                await ctx.ChatMemberships.Where(l => l.ChatId == chatId).ExecuteDeleteAsync();
                await ctx.Chats.Where(c => c.Id == chatId).ExecuteDeleteAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CoreChat> GetChatAsync(long chatId)
        {
            await using CardinalContext ctx = initContext();

            Chat chat = await ctx.Chats.
                                Where(c => c.Id == chatId).
                                SingleAsync();

            long circleId = chat.Type == ChatType.Circle ? ((CircleChat)chat).CircleId : 0;

            return new CoreChat(chat.Id, chat.Type, chat.CreatedAt, circleId == 0 ? circleId : null);
        }

        public async Task<List<CoreMembership>> GetChatMembersAsync(long chatId)
        {
            await using CardinalContext ctx = initContext();

            return await ctx.ChatMemberships.
                   Where(l => l.ChatId == chatId).
                   Select(l => new CoreMembership(l.UserId, l.Type, l.LastSeen, l.Muted)).
                   ToListAsync();
        }

        public async Task<List<CoreChat>> GetChatsForUserAsync(long userId)
        {
            await using CardinalContext ctx = initContext();

            List<Chat> chats = await ctx.ChatMemberships.
                                       Where(l => l.UserId == userId).
                                       Join
                                       (
                                            ctx.Chats,
                                            l => l.ChatId,
                                            c => c.Id,
                                            (_, c) => c
                                       ).
                                       ToListAsync();

            List<CoreChat> toReturn = new();
            foreach (Chat chat in chats)
            {
                CoreChat coreConversation = new(chat.Id, chat.Type, chat.CreatedAt, 0);
                switch (chat)
                {
                    case PrivateChat privateChat:
                        toReturn.Add(coreConversation);
                        break;
                    case CircleChat circleChat:
                        toReturn.Add(coreConversation with { CircleId = circleChat.CircleId });
                        break;
                    default:
                        throw new ArgumentException("Message of type " + chat.GetType().Name + " is not supported by this method.");
                }
            }

            return toReturn;
        }

        public async Task<CoreMembership> GetMembershipAsync(long chatId, long userId)
        {
            await using CardinalContext ctx = initContext();

            return await ctx.ChatMemberships.
                   Where(l => l.ChatId == chatId && l.UserId == userId).
                   Select(l => new CoreMembership(l.UserId, l.Type, l.LastSeen, l.Muted)).
                   SingleAsync();
        }

        public async Task<List<MessageShard>> GetMessagesForChatAsync(long chatId, int pageNumber)
        {
            await using CardinalContext ctx = initContext();

            List<Message> messages = await ctx.Messages.
                                     Where(m => m.ChatId == chatId).
                                     OrderBy(m => m.Timestamp).
                                     Skip(pageNumber * pageSize).
                                     Take(pageSize).
                                     ToListAsync();

            List<MessageShard> toReturn = new();
            foreach (Message message in messages)
            {
                MessageShard messageShard = new(message.Id, message.UserId ?? 0, message.Timestamp, message.Type, null);
                switch (message) 
                {
                    case TextMessage textMessage:
                        toReturn.Add(messageShard with { Value = textMessage.Text });
                        break;
                    case PhotoMessage photoMessage:
                        toReturn.Add(messageShard with { Value = photoMessage.Path });
                        break;
                    case IssueMessage gatheringShareMessage:
                        toReturn.Add(messageShard with { Value = gatheringShareMessage.GatheringId });
                        break;
                    case PostMessage gatheringInviteMessage:
                        toReturn.Add(messageShard with { Value = gatheringInviteMessage.GatheringId });
                        break;
                    case ProfileMessage profileMessage:
                        toReturn.Add(messageShard with { Value = profileMessage.ProfileId });
                        break;
                    case SnapshotMessage snapshotMessage:
                        toReturn.Add(messageShard with { Value = snapshotMessage.SnapshotId });
                        break;
                    case ActivityMessage activityMessage:
                        toReturn.Add(messageShard with { Value = new ActivityMessageShard(activityMessage.ActivityType, activityMessage.ActorId, activityMessage.TargetId, activityMessage.Text) });
                        break;
                    default:
                        throw new ArgumentException("Message of type " + message.GetType().Name + " is not supported by this method.");
                }
            }

            return toReturn;
        }

        public async Task RemoveUserFromChatAsync(long chatId, long userId)
        {
            await using CardinalContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Messages.
                Where(m => m.ChatId == chatId && m.UserId == userId).
                ExecuteUpdateAsync(setter => setter.SetProperty(m => m.UserId, 0));

                await ctx.ChatMemberships.
                Where(l => l.ChatId == chatId && l.UserId == userId).
                ExecuteDeleteAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

           
        }

        public async Task UpdateChatAsync(long chatId, List<(string Property, object Value)> edits)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            GroupChat c = new() { Id = conversationId };
            storeSentry.DiscussWrite(ctx => ctx.Chats.Attach(c), currentDiscussion);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(CoreChat.Title):
                        c.Title = (string)Value;
                        break;
                    default:
                        throw new ArgumentException("Property named \"" + Property + "\" can not be updated using this method.");
                }
                storeSentry.DiscussWrite(ctx => ctx.Entry(c).Property(Property).IsModified = true, currentDiscussion);
            }
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }

        public async Task UpdateMembershipAsync(long chatId, long userId, List<(string Property, object Value)> edits)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            ChatMembership l = await storeSentry.ExecuteReadAsync(ctx => 
                                    ctx.ChatLinks.
                                    Where(l => l.ConversationId == conversationId && l.UserId == userId).
                                    SingleAsync());

            storeSentry.DiscussWrite(ctx => ctx.ChatLinks.Attach(l), currentDiscussion);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(CoreMembership.Muted):
                        l.Muted = (bool)Value;
                        break;
                    case nameof(CoreMembership.LastSeen):
                        l.LastSeen = (DateTimeOffset)Value;
                        break;
                    case nameof(CoreMembership.Type):
                        l.Type = (ChatMembershipType)Value;
                        break;
                    default:
                        throw new InvalidInputException($"Property named \"{Property}\" can not be updated using this method.");
                }
                storeSentry.DiscussWrite(ctx => ctx.Entry(l).Property(Property).IsModified = true, currentDiscussion);
            }
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }

        public async Task<long> CreateGroupChatConversationAsync(ChatType type, string title, DateTimeOffset currentTime)
        {
            if (type != ChatType.OldGC)
            {
                throw new ArgumentException("Message of type " + type.ToString() + " is not supported by this method.");
            }

            GroupChat toAdd = new() { Title = title, Type = type, CreatedAt = currentTime};

            await storeSentry.ExecuteWriteAsync(ctx => ctx.GroupChats.Add(toAdd));

            return toAdd.Id;
        }

        public async Task<CoreChat> GetOrCreateIndividualChatBetween(long userIdA, long userIdB, DateTimeOffset currentTime)
        {
            List<CoreChat> conversations = await storeSentry.ExecuteReadAsync(ctx => 
                ctx.PrivateChats.
                Join(
                    ctx.ChatLinks.Where(l => l.UserId == userIdA || l.UserId == userIdB),
                    c => c.Id,
                    m => m.ConversationId,
                    (c, m) => new CoreChat(c.Id, c.Type, c.CreatedAt, null, 0)
                ).
                ToListAsync());

            List<long> seen = new();
            foreach (CoreChat c in conversations)
            {
                if (seen.Contains(c.Id))
                {
                    return c;
                }
                else
                {
                    seen.Add(c.Id);
                }
            }

            PrivateChat toAdd = new() { Type = ChatType.Individual, CreatedAt = currentTime };

            await storeSentry.ExecuteWriteAsync(ctx => ctx.PrivateChats.Add(toAdd));

            ChatMembership membershipA = new() { UserId = userIdA, ChatId = toAdd.Id, Type = ChatMembershipType.Owner, LastSeen = DateTimeOffset.UtcNow };
            ChatMembership membershipB = new() { UserId = userIdB, ChatId = toAdd.Id, Type = ChatMembershipType.Owner, LastSeen = DateTimeOffset.UtcNow };

            await storeSentry.ExecuteWriteAsync(ctx => ctx.ChatLinks.AddRange(membershipA, membershipB));

            return new CoreChat(toAdd.Id, toAdd.Type, toAdd.CreatedAt);
        }

        public async Task<bool> IndividualChatBetweenExists(long userIdA, long userIdB)
        {
            List<CoreChat> chats = await storeSentry.ExecuteReadAsync(ctx =>
                ctx.PrivateChats.
                Join(
                    ctx.ChatLinks.Where(l => l.UserId == userIdA || l.UserId == userIdB),
                    c => c.Id,
                    l => l.ConversationId,
                    (c, l) => new CoreChat(c.Id, c.Type, c.CreatedAt, null, 0)
                ).
                ToListAsync());

            return chats.Count != chats.Distinct().Count();
        }

        public async Task<bool> CircleChatExists(long gatheringId)
        {
            long chatId = await storeSentry.ExecuteReadAsync(ctx =>
                ctx.GatheringChats.
                Where(c => c.GatheringId == gatheringId).
                Select(c => c.Id).
                SingleOrDefaultAsync());

            return chatId != 0;
        }

        public async Task<CoreChat> GetOrCreateCircleChat(long gatheringId, DateTimeOffset currentTime)
        {
            CoreChat? conversation = await storeSentry.ExecuteReadAsync(ctx =>
               ctx.GatheringChats.
               Where(c => c.GatheringId == gatheringId).
               Select(c => new CoreChat(c.Id, c.Type, c.CreatedAt, null, c.GatheringId)).
               SingleOrDefaultAsync());

            if (conversation != null)
            {
                return conversation;
            }

            CircleChat toAdd = new() { Type = ChatType.Circle, CreatedAt = currentTime, GatheringId = gatheringId };

            await storeSentry.ExecuteWriteAsync(ctx => ctx.GatheringChats.Add(toAdd));

            List<long> guestList = await storeSentry.ExecuteReadAsync(ctx =>
                                    ctx.GatheringLinks.
                                    Where(l => l.GatheringId == gatheringId && l.Type == GatheringBond.Guest).
                                    Select(l => l.UserId).
                                    ToListAsync());

            long? hostId = await storeSentry.ExecuteReadAsync(ctx =>
                             ctx.Gatherings.
                             Where(g => g.Id == gatheringId).
                             Select(g => g.HostId).
                             SingleAsync());

            List<ChatMembership> links = new();
            foreach (long userId in guestList)
            {
                links.Add(new() { UserId = userId, ChatId = toAdd.Id, Type = hostId == userId ? ChatMembershipType.Owner : ChatMembershipType.Regular, LastSeen = DateTimeOffset.UtcNow });
            }
            await storeSentry.ExecuteWriteAsync(ctx => ctx.ChatLinks.AddRange(links));

            return new CoreChat(toAdd.Id, toAdd.Type, toAdd.CreatedAt, null, toAdd.GatheringId);
        }

        public async Task<long> CreateGroupChatConversationAsync(DateTimeOffset currentTime, string title)
        {
            GroupChat toAdd = new() { Type = ChatType.OldGC, Title = title, CreatedAt = currentTime };

            await storeSentry.ExecuteWriteAsync(ctx => ctx.GroupChats.Add(toAdd));

            return toAdd.Id;
        }

        public async Task<int> GetLastPageNumber(long chatId)
        {
            int messageCount = await storeSentry.ExecuteReadAsync(ctx => 
                                ctx.Messages.
                                Where(m => m.ConversationId == conversationId).
                                CountAsync());

            int totalPages = (messageCount + pageSize - 1) / pageSize;

            return Math.Max(0, totalPages - 1);
        }

        public async Task<MessageShard> GetMessagesSinceAsync(long chatId)
        {
            Message? message =  await storeSentry.ExecuteReadAsync(ctx => 
                                    ctx.Messages
                                    .Where(m => m.ConversationId == conversationId)
                                    .OrderByDescending(m => m.Timestamp)
                                    .FirstOrDefaultAsync());

            MessageShard messageShard = new(message.Id, message.UserId ?? 0, message.Timestamp, message.Type, null);
            switch (message)
            {
                case TextMessage textMessage:
                    return messageShard with { Value = textMessage.Text };
                case PhotoMessage imageMessage:
                    return messageShard with { Value = imageMessage.StorageId };
                case IssueMessage gatheringShareMessage:
                    return messageShard with { Value = gatheringShareMessage.GatheringId };
                case PostMessage gatheringInviteMessage:
                    return messageShard with { Value = gatheringInviteMessage.GatheringId };
                case ProfileMessage profileMessage:
                    return messageShard with { Value = profileMessage.ProfileId };
                case SnapshotMessage snapshotMessage:
                    return messageShard with { Value = snapshotMessage.SnapshotId };
                case ActivityMessage activityMessage:
                    return messageShard with { Value = new ActivityMessageShard(activityMessage.ActivityType, activityMessage.ActorId, activityMessage.TargetId, activityMessage.Text) };
                default:
                    throw new ArgumentException("Message of type " + message.GetType().Name + " is not supported by this method.");
            }
        }

        public Task<int> GetMessageCountSinceAsync(long chatId, DateTimeOffset timestamp)
        {
            return storeSentry.ExecuteReadAsync(ctx => 
                    ctx.Messages
                    .CountAsync(m => m.ConversationId == conversationId && m.Timestamp >= timestamp));
        }
    }
}

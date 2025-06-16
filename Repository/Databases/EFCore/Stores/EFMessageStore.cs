using Microsoft.EntityFrameworkCore;

namespace Repository
{
    class EFMessageStore : QueryStore, IMessageDatabase
    {
        private int pageSize = 10;

        public EFMessageStore(Harbor.Flag flag) : base(flag)
        {
        }


        public async Task<MessageShard> AddMessageAsync(long conversationId, long userId, DateTimeOffset timestamp, MessageType type, object value)
        {
            Message toAdd;

            switch (type)
            {
                case MessageType.Text:
                    toAdd = new TextMessage()
                    {
                        ConversationId = conversationId,
                        UserId = userId,
                        Timestamp = timestamp,
                        Text = (string)value,
                    };
                    break;
                case MessageType.Photo:
                    toAdd = new ImageMessage()
                    {
                        ConversationId = conversationId,
                        UserId = userId,
                        Timestamp = timestamp,
                        StorageId = (Guid)value
                    };
                    break;
                case MessageType.ShareGathering:
                    toAdd = new GatheringShareMessage()
                    {
                        ConversationId = conversationId,
                        UserId = userId,
                        Timestamp = timestamp,
                        GatheringId = (long)value
                    };
                    break;
                case MessageType.GatheringInvite:
                    toAdd = new GatheringInviteMessage()
                    {
                        ConversationId = conversationId,
                        UserId = userId,
                        Timestamp = timestamp,
                        GatheringId = (long)value
                    };
                    break;
                case MessageType.Snapshot:
                    toAdd = new SnapshotMessage()
                    {
                        ConversationId = conversationId,
                        UserId = userId,
                        Timestamp = timestamp,
                        SnapshotId = (long)value
                    };
                    break;
                case MessageType.Nest:
                    toAdd = new ProfileMessage()
                    {
                        ConversationId = conversationId,
                        UserId = userId,
                        Timestamp = timestamp,
                        ProfileId = (long)value
                    };
                    break;
                case MessageType.Activity:
                    ActivityMessageShard activityMessageShard = (ActivityMessageShard)value;
                    toAdd = new ActivityMessage()
                    {
                        ConversationId = conversationId,
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

            await storeSentry.ExecuteWriteAsync(ctx => ctx.Messages.Add(toAdd));
            return new MessageShard(toAdd.Id, toAdd.UserId ?? 0, toAdd.Timestamp, type, value);
        }

        public async Task AddUsersToConversationAsync(long conversationId, params long[] userIds)
        {
            Discussion discussion = storeSentry.BeginDiscussion();

            foreach (long userId in userIds)
            {
                storeSentry.DiscussWrite(ctx => 
                    ctx.ChatLinks.
                    Add(
                        new ChatLink 
                        { 
                            ConversationId = conversationId, 
                            UserId = userId,
                            LastSeen = DateTime.UtcNow,
                            Type = MembershipType.Regular
                        }
                ), discussion);
            }

            await storeSentry.EndDiscussionAsync(discussion);
        }

        public async Task DeleteConversationAsync(long conversationId)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Messages.
               Where(m => m.ConversationId == conversationId).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.ChatLinks.
               Where(l => l.ConversationId == conversationId).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Chats.
               Where(c => c.Id == conversationId).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));
        }

        public async Task<CoreConversation> GetConversationAsync(long conversationId)
        {
            Chat conversation = await storeSentry.ExecuteReadAsync(ctx =>
                                            ctx.Chats.
                                            Where(c => c.Id == conversationId).
                                            SingleAsync());

            string? title = conversation.Type == ChatType.Group ? ((GroupChat)conversation).Title : null;
            long gatheringId = conversation.Type == ChatType.Gathering ? ((GatheringChat)conversation).GatheringId : 0;

            return new CoreConversation(conversation.Id, conversation.Type, conversation.CreatedAt, title, gatheringId);
        }

        public async Task<List<CoreMembership>> GetConversationMembersAsync(long conversationId)
        {
            return await storeSentry.ExecuteReadAsync(ctx => 
                    ctx.ChatLinks.
                    Where(l => l.ConversationId == conversationId).
                    Select(l => new CoreMembership(l.UserId, l.Type, l.LastSeen, l.Muted)).
                    ToListAsync());
        }

        public async Task<List<CoreConversation>> GetConversationsForUserAsync(long userId)
        {
            List<Chat> conversations = await storeSentry.ExecuteReadAsync(ctx =>
                                                    ctx.ChatLinks.
                                                    Where(l => l.UserId == userId).
                                                    Join(
                                                        ctx.Chats,
                                                        l => l.ConversationId,
                                                        c => c.Id,
                                                        (l, c) => c
                                                    ).
                                                    ToListAsync());

            List<CoreConversation> toReturn = new();
            foreach (Chat conversation in conversations)
            {
                CoreConversation coreConversation = new(conversation.Id, conversation.Type, conversation.CreatedAt, null, 0);
                switch (conversation)
                {
                    case PrivateChat privateChat:
                        toReturn.Add(coreConversation);
                        break;
                    case GroupChat groupChat:
                        toReturn.Add(coreConversation with { Title = groupChat.Title });
                        break;
                    case GatheringChat gatheringChat:
                        toReturn.Add(coreConversation with { GatheringId = gatheringChat.GatheringId });
                        break;
                    default:
                        throw new ArgumentException("Message of type " + conversation.GetType().Name + " is not supported by this method.");
                }
            }

            return toReturn;
        }

        public async Task<CoreMembership> GetMembershipAsync(long conversationId, long userId)
        {
            return await storeSentry.ExecuteReadAsync(ctx => 
                    ctx.ChatLinks.
                    Where(l => l.ConversationId == conversationId && l.UserId == userId).
                    Select(l => new CoreMembership(l.UserId, l.Type, l.LastSeen, l.Muted)).
                    SingleAsync());
        }

        public async Task<List<MessageShard>> GetMessagesForConversationAsync(long conversationId, int pageNumber)
        {
            List<Message> messages = await storeSentry.ExecuteReadAsync(ctx =>
                                        ctx.Messages
                                        .Where(m => m.ConversationId == conversationId)
                                        .OrderBy(m => m.Timestamp)
                                        .Skip((pageNumber) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync());

            List<MessageShard> toReturn = new();
            foreach (Message message in messages)
            {
                MessageShard messageShard = new(message.Id, message.UserId ?? 0, message.Timestamp, message.Type, null);
                switch (message) 
                {
                    case TextMessage textMessage:
                        toReturn.Add(messageShard with { Value = textMessage.Text });
                        break;
                    case ImageMessage imageMessage:
                        toReturn.Add(messageShard with { Value = imageMessage.StorageId });
                        break;
                    case GatheringShareMessage gatheringShareMessage:
                        toReturn.Add(messageShard with { Value = gatheringShareMessage.GatheringId });
                        break;
                    case GatheringInviteMessage gatheringInviteMessage:
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

        public async Task RemoveUserFromConversationAsync(long conversationId, long userId)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.ChatLinks.
               Where(l => l.ConversationId == conversationId && l.UserId == userId).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));
        }

        public async Task UpdateConversationAsync(long conversationId, List<(string Property, object Value)> edits)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            GroupChat c = new() { Id = conversationId };
            storeSentry.DiscussWrite(ctx => ctx.Chats.Attach(c), currentDiscussion);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(CoreConversation.Title):
                        c.Title = (string)Value;
                        break;
                    default:
                        throw new ArgumentException("Property named \"" + Property + "\" can not be updated using this method.");
                }
                storeSentry.DiscussWrite(ctx => ctx.Entry(c).Property(Property).IsModified = true, currentDiscussion);
            }
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }

        public async Task UpdateMembershipAsync(long conversationId, long userId, List<(string Property, object Value)> edits)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            ChatLink l = await storeSentry.ExecuteReadAsync(ctx => 
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
                        l.Type = (MembershipType)Value;
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
            if (type != ChatType.Group)
            {
                throw new ArgumentException("Message of type " + type.ToString() + " is not supported by this method.");
            }

            GroupChat toAdd = new() { Title = title, Type = type, CreatedAt = currentTime};

            await storeSentry.ExecuteWriteAsync(ctx => ctx.GroupChats.Add(toAdd));

            return toAdd.Id;
        }

        public async Task<CoreConversation> GetOrCreateIndividualConversationBetween(long userIdA, long userIdB, DateTimeOffset currentTime)
        {
            List<CoreConversation> conversations = await storeSentry.ExecuteReadAsync(ctx => 
                ctx.PrivateChats.
                Join(
                    ctx.ChatLinks.Where(l => l.UserId == userIdA || l.UserId == userIdB),
                    c => c.Id,
                    m => m.ConversationId,
                    (c, m) => new CoreConversation(c.Id, c.Type, c.CreatedAt, null, 0)
                ).
                ToListAsync());

            List<long> seen = new();
            foreach (CoreConversation c in conversations)
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

            ChatLink membershipA = new() { UserId = userIdA, ConversationId = toAdd.Id, Type = MembershipType.Owner, LastSeen = DateTimeOffset.UtcNow };
            ChatLink membershipB = new() { UserId = userIdB, ConversationId = toAdd.Id, Type = MembershipType.Owner, LastSeen = DateTimeOffset.UtcNow };

            await storeSentry.ExecuteWriteAsync(ctx => ctx.ChatLinks.AddRange(membershipA, membershipB));

            return new CoreConversation(toAdd.Id, toAdd.Type, toAdd.CreatedAt);
        }

        public async Task<bool> IndividualConversationBetweenExists(long userIdA, long userIdB)
        {
            List<CoreConversation> chats = await storeSentry.ExecuteReadAsync(ctx =>
                ctx.PrivateChats.
                Join(
                    ctx.ChatLinks.Where(l => l.UserId == userIdA || l.UserId == userIdB),
                    c => c.Id,
                    l => l.ConversationId,
                    (c, l) => new CoreConversation(c.Id, c.Type, c.CreatedAt, null, 0)
                ).
                ToListAsync());

            return chats.Count != chats.Distinct().Count();
        }

        public async Task<bool> GatheringConversationExists(long gatheringId)
        {
            long chatId = await storeSentry.ExecuteReadAsync(ctx =>
                ctx.GatheringChats.
                Where(c => c.GatheringId == gatheringId).
                Select(c => c.Id).
                SingleOrDefaultAsync());

            return chatId != 0;
        }

        public async Task<CoreConversation> GetOrCreateGatheringConversation(long gatheringId, DateTimeOffset currentTime)
        {
            CoreConversation? conversation = await storeSentry.ExecuteReadAsync(ctx =>
               ctx.GatheringChats.
               Where(c => c.GatheringId == gatheringId).
               Select(c => new CoreConversation(c.Id, c.Type, c.CreatedAt, null, c.GatheringId)).
               SingleOrDefaultAsync());

            if (conversation != null)
            {
                return conversation;
            }

            GatheringChat toAdd = new() { Type = ChatType.Gathering, CreatedAt = currentTime, GatheringId = gatheringId };

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

            List<ChatLink> links = new();
            foreach (long userId in guestList)
            {
                links.Add(new() { UserId = userId, ConversationId = toAdd.Id, Type = hostId == userId ? MembershipType.Owner : MembershipType.Regular, LastSeen = DateTimeOffset.UtcNow });
            }
            await storeSentry.ExecuteWriteAsync(ctx => ctx.ChatLinks.AddRange(links));

            return new CoreConversation(toAdd.Id, toAdd.Type, toAdd.CreatedAt, null, toAdd.GatheringId);
        }

        public async Task<long> CreateGroupChatConversationAsync(DateTimeOffset currentTime, string title)
        {
            GroupChat toAdd = new() { Type = ChatType.Group, Title = title, CreatedAt = currentTime };

            await storeSentry.ExecuteWriteAsync(ctx => ctx.GroupChats.Add(toAdd));

            return toAdd.Id;
        }

        public async Task<int> GetLastPageNumber(long conversationId)
        {
            int messageCount = await storeSentry.ExecuteReadAsync(ctx => 
                                ctx.Messages.
                                Where(m => m.ConversationId == conversationId).
                                CountAsync());

            int totalPages = (messageCount + pageSize - 1) / pageSize;

            return Math.Max(0, totalPages - 1);
        }

        public async Task<MessageShard> GetMessagesSinceAsync(long conversationId)
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
                case ImageMessage imageMessage:
                    return messageShard with { Value = imageMessage.StorageId };
                case GatheringShareMessage gatheringShareMessage:
                    return messageShard with { Value = gatheringShareMessage.GatheringId };
                case GatheringInviteMessage gatheringInviteMessage:
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

        public Task<int> GetMessageCountSinceAsync(long conversationId, DateTimeOffset timestamp)
        {
            return storeSentry.ExecuteReadAsync(ctx => 
                    ctx.Messages
                    .CountAsync(m => m.ConversationId == conversationId && m.Timestamp >= timestamp));
        }
    }
}

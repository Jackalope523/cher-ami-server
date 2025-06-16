using System.Collections.Generic;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using System.Linq;

using static Core.Entities.Arbiter;
using static Core.Entities.Psijic;
using static Core.Entities.Smithing;
using System;

namespace Core.Controls
{
    internal class MessageDirector : AbstractDirector, IMessageOperations
    {
        #region Initialisation

        public MessageDirector(CoreTerminal terminal) : base(terminal) { }

        #endregion

        #region Operations

        public async Task<List<ConversationShard>> GetConversationsAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            var conversations = await Psijic.Once((await user.Conversations)
                .Select(c => c.Conversation.ToConversationShard(c.Membership)));

            return conversations.ToList();
        }

        public async Task<ConversationShard> GetAnnouncementsAsync(long userId, string locale)
        {
            var user = await GetUserAsync(userId);

            var conversation = await GetConversationAsync(-2);

            return await conversation.ToConversationShard();
        }

        public async Task<ConversationShard> GetGatheringConversationAsync(long userId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
            var gathering = await GetGatheringAsync(gatheringId);

            // todo checks

            var exists = await Messages.GatheringConversationExists(gathering.Id);

            Conversation conversation = Conversation.None;

            if (exists)
            {
                conversation = new(await Messages.GetOrCreateGatheringConversation(gathering.Id, Time));
            }

            return await conversation.ToConversationShard();
        }

        public async Task<ConversationShard> GetOrCreateGatheringConversationAsync(long userId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
            var gathering = await GetGatheringAsync(gatheringId);

            // todo checks

            Conversation conversation = new(await Messages.GetOrCreateGatheringConversation(gathering.Id, Time));

            return await conversation.ToConversationShard();
        }

        public async Task<ConversationShard> GetConversationWithAsync(long userId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var target = await GetUserAsync(targetId);

            // todo checks

            var exists = await Messages.IndividualConversationBetweenExists(user.Id, target.Id);

            Conversation conversation = Conversation.None;

            if (exists)
            {
                conversation = new(await Messages.GetOrCreateIndividualConversationBetween(user.Id, target.Id, Time));
            }

            return await conversation.ToConversationShard();
        }

        public async Task<ConversationShard> GetOrCreateConversationWithAsync(long userId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var target = await GetUserAsync(targetId);

            // todo checks

            Conversation conversation = new(await Messages.GetOrCreateIndividualConversationBetween(user.Id, target.Id, Time));

            return await conversation.ToConversationShard();
        }

        public async Task<ConversationShard> GetConversationAsync(long userId, long conversationId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.VisibleTo(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            return await conversation.ToConversationShard();
        }

        public async Task<List<MembershipShard>> GetMembersAsync(long userId, long conversationId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.VisibleTo(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            return (await conversation.Members)
                .ConvertAll(m => m.Membership.ToShard());
        }

        public async Task<List<MessageShard>> GetMessagesAsync(long userId, long conversationId, int pageNumber)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.VisibleTo(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            return await conversation.Messages.Value(pageNumber);
        }

        public async Task UserReadAsync(long userId, long conversationId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            // We don't want to track individual seens
            if (conversation.Type == ChatType.Broadcast)
            { return; }

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            await Messages.UpdateMembershipAsync(conversation.Id, user.Id, new() { (nameof(CoreMembership.LastSeen), Time) });
        }

        public async Task UserComposingAsync(long userId, long conversationId, bool isComposing)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            _ = conversation.IndicateUserComposingAsync(user, isComposing);
        }

        public async Task<MessageShard> SendTextAsync(long userId, long conversationId, string text)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            FailIf(string.IsNullOrWhiteSpace(text),
                new UserErrorException(ConversationErrorCode.EMPTY_MESSAGE));

            var message = await Messages.AddMessageAsync(conversation.Id, user.Id, Time, MessageType.Text, text);

            _ = conversation.MessageOrNotifyOthersAsync(user, message);

            await Messages.UpdateMembershipAsync(conversation.Id, user.Id, new() { (nameof(CoreMembership.LastSeen), Time) });

            return message;
        }

        public async Task<MessageShard> SendPhotoAsync(long userId, long conversationId, MemoryStream photo)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            var photoId = await Media.UploadPhotoAsync(conversation.Id, photo);

            var message = await Messages.AddMessageAsync(conversation.Id, user.Id, Time, MessageType.Photo, photoId);

            _ = conversation.MessageOrNotifyOthersAsync(user, message);

            await Messages.UpdateMembershipAsync(conversation.Id, user.Id, new() { (nameof(CoreMembership.LastSeen), Time) });

            return message;
        }

        public async Task<MessageShard> InviteToGatheringAsync(long userId, long conversationId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            var gathering = await GetGatheringAsync(gatheringId);
            
            var message = await Messages.AddMessageAsync(conversation.Id, user.Id, Time, MessageType.GatheringInvite, gathering.Id);

            _ = conversation.MessageOrNotifyOthersAsync(user, message);

            await Messages.UpdateMembershipAsync(conversation.Id, user.Id, new() { (nameof(CoreMembership.LastSeen), Time) });

            return message;
        }

        public async Task<MessageShard[]> ShareGatheringAsync(long userId, long conversationId, long[] gatheringIds)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            List<MessageShard> messages = new();
            var time = Time;

            foreach (var gatheringId in gatheringIds)
            {
                var gathering = await GetGatheringAsync(gatheringId);

                Verify(await user.CanView(gathering),
                    new UserErrorException(GatheringErrorCode.CANNOT_VIEW));

                messages.Add(await Messages.AddMessageAsync(conversation.Id, user.Id, time, MessageType.ShareGathering, gathering.Id));
            }

            _ = conversation.BulkMessageOrNotifyOthersAsync(user, messages);

            await Messages.UpdateMembershipAsync(conversation.Id, user.Id, new() { (nameof(CoreMembership.LastSeen), Time) });

            return messages.ToArray();
        }

        public async Task<MessageShard[]> ShareSnapshotAsync(long userId, long conversationId, long[] snapshotIds)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            List<MessageShard> messages = new();
            var time = Time;

            foreach (var snapshotId in snapshotIds)
            {
                var snapshot = await Snapshots.GetSnapshotAsync(snapshotId);
                User snapshotOwner = await GetUserAsync(snapshot.User.Id);
                var etchedGathering = await GetGatheringAsync(snapshot.GatheringId);

                Verify(user.Taken(snapshot) ||
                    await user.IsCompanionsWith(snapshotOwner) ||
                    await etchedGathering.HasOnGuestList(user),
                    new UserErrorException(SnapshotErrorCode.CANNOT_VIEW));

                messages.Add(await Messages.AddMessageAsync(conversation.Id, user.Id, time, MessageType.Snapshot, snapshot.Id));
            }

            _ = conversation.BulkMessageOrNotifyOthersAsync(user, messages);

            await Messages.UpdateMembershipAsync(conversation.Id, user.Id, new() { (nameof(CoreMembership.LastSeen), Time) });

            return messages.ToArray();
        }

        public async Task<MessageShard[]> ShareNestAsync(long userId, long conversationId, long[] nestIds)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            List<MessageShard> messages = new();
            var time = Time;

            foreach (var nestId in nestIds)
            {
                var nest = await GetUserAsync(nestId);

                FailIf(await user.IsBlockedBy(nest),
                    new UserErrorException(UserErrorCode.CANNOT_VIEW));

                messages.Add(await Messages.AddMessageAsync(conversation.Id, user.Id, time, MessageType.Nest, nest.Id));
            }

            _ = conversation.BulkMessageOrNotifyOthersAsync(user, messages);

            await Messages.UpdateMembershipAsync(conversation.Id, user.Id, new() { (nameof(CoreMembership.LastSeen), Time) });

            return messages.ToArray();
        }

        public async Task<ConversationShard> CreateGroupChatAsync(long userId, params long[] participantIds)
        {
            var user = await GetUserAsync(userId);

            HashSet<long> uniqueIds = new(participantIds.Append(user.Id));
            
            foreach (var id in uniqueIds)
            {
                var target = await GetUserAsync(id);

                // todo checks
            }

            var conversation = await GetConversationAsync(await Messages.CreateGroupChatConversationAsync(Time));

            await Messages.AddUsersToConversationAsync(conversation.Id, uniqueIds.ToArray());
            await Messages.UpdateMembershipAsync(conversation.Id, user.Id, new() { (nameof(CoreMembership.Type), MembershipType.Owner) });

            ActivityMessageShard activity = new(ActivityMessageType.Initiated, ActorId: user.Id);
            await Messages.AddMessageAsync(conversation.Id, User.Hollow.Id, Time, MessageType.Activity, activity);
            
            return await conversation.ToConversationShard();
        }

        public async Task EditGroupChatAsync(long userId, long conversationId,
            string title = "", MemoryStream header = null)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            Verify(await conversation.IsModifiableBy(user),
                new UserErrorException(ConversationErrorCode.CANNOT_EDIT_PERMISSION));

            Conversation editedConversation = new(conversation.ToCoreConversation())
            {
                Title = title,
            };

            // Validate conversation
            Verify(editedConversation.ValidateAndNormalise(out string issues),
                new UserErrorException(ConversationErrorCode.INVALID_DETAILS, new { issues }));

            List<(string Property, object Value)> edits = new();
            List<ActivityMessageShard> editMessages = new();

            if (!string.IsNullOrEmpty(title))
            {
                edits.Add((nameof(CoreConversation.Title), editedConversation.Title));
                editMessages.Add(new(ActivityMessageType.Edited, ActorId: user.Id, Info: "title"));
            }

            if (header != null && header.Length > 0)
            {
                await Terminal.MediaDirector.UploadGroupChatHeaderAsync(conversation.Id, header);
                editMessages.Add(new(ActivityMessageType.Edited, ActorId: user.Id, Info: "header"));
            }

            if (edits.Any())
            {
                await Messages.UpdateConversationAsync(conversation.Id, edits);
            }

            if (editMessages.Any())
            {
                foreach (var value in editMessages)
                {
                    var message = await Messages.AddMessageAsync(conversation.Id, User.Hollow.Id, Time, MessageType.Activity, value);
                    _ = conversation.MessageOthersAsync(User.Hollow, message);
                }
            }
        }

        public async Task DeleteGroupChatAsync(long userId, long conversationId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(conversation.Type == ChatType.Group,
                new UserErrorException(ConversationErrorCode.NOT_GROUP_CHAT));

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            Verify(await conversation.IsModifiableBy(user),
                new UserErrorException(ConversationErrorCode.CANNOT_DELETE_PERMISSION));

            await Messages.DeleteConversationAsync(conversation.Id);
        }

        public async Task LeaveGroupChatAsync(long userId, long conversationId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(conversation.Type == ChatType.Group,
                new UserErrorException(ConversationErrorCode.NOT_GROUP_CHAT));

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            await Messages.RemoveUserFromConversationAsync(conversation.Id, user.Id);

            ActivityMessageShard activityMessage = new(ActivityMessageType.Left, ActorId: user.Id);
            var message = await Messages.AddMessageAsync(conversation.Id, User.Hollow.Id, Time, MessageType.Activity, activityMessage);

            _ = conversation.MessageOthersAsync(User.Hollow, message);
        }

        public async Task SummonUserAsync(long userId, long conversationId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var summoned = await GetUserAsync(targetId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(conversation.Type == ChatType.Group,
                new UserErrorException(ConversationErrorCode.NOT_GROUP_CHAT));

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            Verify(await user.IsCompanionsWith(summoned),
                new UserErrorException(ConversationErrorCode.CANNOT_INVITE_NEUTRAL));

            await Messages.AddUsersToConversationAsync(conversation.Id, summoned.Id);

            ActivityMessageShard activityMessage = new(ActivityMessageType.Summoned, ActorId: user.Id, TargetId: summoned.Id);
            var message = await Messages.AddMessageAsync(conversation.Id, User.Hollow.Id, Time, MessageType.Activity, activityMessage);

            _ = conversation.MessageOthersAsync(User.Hollow, message);
        }

        public async Task KickUserAsync(long userId, long conversationId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(conversation.Type == ChatType.Group,
                new UserErrorException(ConversationErrorCode.NOT_GROUP_CHAT));

            Verify(await conversation.HasMember(user),
                new UserErrorException(ConversationErrorCode.NOT_MEMBER));

            Verify(await conversation.IsModifiableBy(user),
                new UserErrorException(ConversationErrorCode.CANNOT_KICK_PERMISSION));

            await Messages.RemoveUserFromConversationAsync(conversation.Id, targetId);

            ActivityMessageShard activityMessage = new(ActivityMessageType.Kicked, ActorId: user.Id, TargetId: targetId);
            var message = await Messages.AddMessageAsync(conversation.Id, User.Hollow.Id, Time, MessageType.Activity, activityMessage);

            _ = conversation.MessageOthersAsync(User.Hollow, message);
        }

        #endregion

        #region Favours

        public async Task<List<(Conversation, CoreMembership)>> RequestConversationsForUserAsync(User user)
        {
            var conversations = await Messages.GetConversationsForUserAsync(user.Id);
            var pairs = await Psijic.Once(conversations
                .Select(async c => (new Conversation(c), await Messages.GetMembershipAsync(c.Id, user.Id))));

            return pairs.ToList();
        }

        public async Task<int> RequestConversationPageCountAsync(Conversation conversation)
        {
            return await Messages.GetLastPageNumber(conversation.Id);
        }

        public async Task<List<(User, CoreMembership)>> RequestConversationMembersAsync(Conversation conversation)
        {
            var members = await Messages.GetConversationMembersAsync(conversation.Id);
            var pairs = await Psijic.Once(members
                .Select(async m => (await User.GetUserAsync(m.UserId), m)));

            return pairs.ToList();
        }

        public async Task<List<MessageShard>> RequestConversationMessagesAsync(Conversation conversation, int page)
        {
            return await Messages.GetMessagesForConversationAsync(conversation.Id, page);
        }

        public async Task<int> RequestMessageCountSinceAsync(Conversation conversation, DateTimeOffset timestamp)
        {
            return await Messages.GetMessageCountSinceAsync(conversation.Id, timestamp);
        }

        public async Task SendClientMessageAsync(Conversation conversation, MessageShard message, params User[] users)
        {
            string[] connectionIds = (await Psijic.Once(users
                .Select(async u => await u.Connections)))
                .SelectMany(c => c)
                .ToArray();

            await Terminal.SocketService.BroadcastAsync(client => client.ReceiveMessage(conversation.Id, message),
                connectionIds);
        }

        public async Task SendClientMessagesAsync(Conversation conversation, MessageShard[] messages, params User[] users)
        {
            string[] connectionIds = (await Psijic.Once(users
                .Select(async u => await u.Connections)))
                .SelectMany(c => c)
                .ToArray();

            await Terminal.SocketService.BroadcastAsync(client => client.ReceiveMessages(conversation.Id, messages),
                connectionIds);
        }

        #endregion

        #region Tools

        #endregion
    }
}

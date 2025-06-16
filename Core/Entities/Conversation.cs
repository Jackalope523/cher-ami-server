using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;

using static Core.Entities.Arbiter;
using static Core.Entities.Psijic;
using Microsoft.Extensions.Logging;
using Core.Notifications;
using Microsoft.VisualBasic;
using System.Reflection;

namespace Core.Entities
{
    using static CoreTerminal;

    internal class Conversation
    {
		#region Variables

		//////
		// Constants
		//////////////

		public const int MaximumTitleLength = 30;

        public static Conversation None
            => new() { Id = -1, Type = ChatType.Individual, Title = "" };

        ///////
        // Properties
        ///////////////

        public long Id { get; init; }
        public ChatType Type { get; init; }
        public DateTimeOffset DateCreated { get; init; }
        public string Title { get; set; }

        public long? GatheringId { get; init; }

        ////////
        // Synced Properties
        //////////////////////
        
        public Synced<int> PageCount { get; }
        public Synced<List<(User User, CoreMembership Membership)>> Members { get; }
        public PagedSync<List<MessageShard>> Messages { get; }

        public Synced<Gathering> Gathering { get; }

        #endregion

        #region Initialisation & Extraction

        public Conversation()
        {
            PageCount = new(() => Terminal.MessageDirector.RequestConversationPageCountAsync(this));
            Members = new(() => Terminal.MessageDirector.RequestConversationMembersAsync(this));
            Messages = new((int page) => Terminal.MessageDirector.RequestConversationMessagesAsync(this, page));

            Gathering = new(() => GatheringId.HasValue ? Entities.Gathering.GetGatheringAsync(GatheringId.Value) : Task.FromResult(Entities.Gathering.None));
        }

        public Conversation(CoreConversation fromConversation) : this()
        {
            Id = fromConversation.Id;
            Type = fromConversation.Type;
            DateCreated = fromConversation.DateCreated;
            Title = fromConversation.Title;
            GatheringId = fromConversation.GatheringId;
        }

        public CoreConversation ToCoreConversation()
        {
            return new(Id, Type, DateCreated, Title);
        }

        public async Task<ConversationShard> ToConversationShard()
        {
            return new(Id, Type, await PageCount, Title, GatheringId);
        }

        public async Task<ConversationShard> ToConversationShard(User relativeTo)
        {
            Verify(await HasMember(relativeTo),
                new UnexpectedFailureException("ToConversationShard: User is not member"));

            var userMembership = (await Members).Find(member => member.User.Equals(relativeTo));

            var lastSeen = userMembership.Membership.LastSeen;
            var unreadCount = await Terminal.MessageDirector.RequestMessageCountSinceAsync(this, lastSeen);

            return new(Id, Type, await PageCount, Title, GatheringId,
                Muted: userMembership.Membership.Muted,
                Unread: unreadCount);
        }

        public async Task<ConversationShard> ToConversationShard(CoreMembership relativeTo)
        {
            var lastSeen = relativeTo.LastSeen;
            var unreadCount = await Terminal.MessageDirector.RequestMessageCountSinceAsync(this, lastSeen);

            return new(Id, Type, await PageCount, Title, GatheringId,
                Muted: relativeTo.Muted,
                Unread: unreadCount);
        }

		#endregion

		#region Composition

		public bool ValidateAndNormalise(out string issues)
        {
            issues = "";

            // Sanitise
            if (!string.IsNullOrEmpty(Title))
            {
                Title = ContentValidation.NormaliseText(Title, MaximumTitleLength);
            }

            return issues.Equals("");
        }

		#endregion

		#region Checks

        public async Task<bool> IsModifiableBy(User user)
        {
            Verify(await HasMember(user),
                new UnexpectedFailureException("IsModifiableBy: User is not member"));

            // Check if user has priviledges
            var userMembership = (await Members).Find(member => member.User.Equals(user));

            if (Type == ChatType.Individual ||
                userMembership.Membership.Type.Equals(MembershipType.Owner))
			{ return true; }

			return false;
        }

        public async Task<bool> VisibleTo(User user)
        {
            if (Type == ChatType.Broadcast)
            { return true; }

            return await HasMember(user);
        }

        public async Task<bool> HasMember(User user)
        {
            // Check if user is affiliated with conversation
            return (await Members).Exists(u => u.User.Equals(user));
        }

        #endregion

        #region Effects

        #endregion

        #region Actions

        public async Task MessageOthersAsync(User sender, MessageShard message)
        {
            var otherMembers = (await Members).Where(m => !m.User.Equals(sender));

            var (onlineMembers, _) = await otherMembers.PartitionAsync(async (member) => await member.User.IsOnline());

            if (onlineMembers.Any())
            {
                await Terminal.MessageDirector.SendClientMessageAsync(this, message, onlineMembers.Select(u => u.User).ToArray());
            }
        }

        public async Task MessageOrNotifyOthersAsync(User sender, MessageShard message)
        {
            var otherMembers = (await Members).Where(m => !m.User.Equals(sender));

            var (onlineMembers, offlineMembers) = await otherMembers.PartitionAsync(async (member) => await member.User.IsOnline());

            if (onlineMembers.Any())
            {
                await Terminal.MessageDirector.SendClientMessageAsync(this, message, onlineMembers.Select(u => u.User).ToArray());
            }

            if (offlineMembers.Any())
            {
                var subscribedMembers = offlineMembers
                    .Where(member => !member.Membership.Muted)
                    .Select(u => u.User)
                    .ToArray();

                var shard = await ToConversationShard();

                CanaryNotification notification = Type switch
                {
                    ChatType.Individual => CanaryNotification.IndividualMessage(shard, sender.ToUserShard(), message),
                    ChatType.Group => CanaryNotification.GroupMessage(shard, sender.ToUserShard(), message),
                    ChatType.Gathering => CanaryNotification.GatheringMessage(await (await Gathering).ToGatheringShard(), shard, sender.ToUserShard(), message),
                    _ => throw new UnexpectedFailureException("ConversationType does not exist"),
                };

                await User.NotifyAll(notification, subscribedMembers);
            }
        }

        public async Task BulkMessageOrNotifyOthersAsync(User sender, List<MessageShard> messages)
        {
            var otherMembers = (await Members).Where(m => !m.User.Equals(sender));

            var (onlineMembers, offlineMembers) = await otherMembers.PartitionAsync(async (member) => await member.User.IsOnline());

            if (onlineMembers.Any())
            {
                await Terminal.MessageDirector.SendClientMessagesAsync(this, messages.ToArray(), onlineMembers.Select(u => u.User).ToArray());
            }

            if (offlineMembers.Any())
            {
                var subscribedMembers = offlineMembers
                    .Where(member => !member.Membership.Muted)
                    .Select(u => u.User)
                    .ToArray();

                var shard = await ToConversationShard();

                if (messages.Any())
                {
                    CanaryNotification notification = Type switch
                    {
                        ChatType.Individual => CanaryNotification.IndividualMessage(shard, sender.ToUserShard(), messages.First()),
                        ChatType.Group => CanaryNotification.GroupMessage(shard, sender.ToUserShard(), messages.First()),
                        ChatType.Gathering => CanaryNotification.GatheringMessage(await (await Gathering).ToGatheringShard(), shard, sender.ToUserShard(), messages.First()),
                        _ => throw new UnexpectedFailureException("ConversationType does not exist"),
                    };

                    await User.NotifyAll(notification, subscribedMembers);
                }
            }
        }

        public async Task IndicateUserComposingAsync(User user, bool isComposing)
        {
            var otherMembers = (await Members).Where(m => !m.User.Equals(user));

            var (onlineMembers, _) = await otherMembers.PartitionAsync(async (member) => await member.User.IsOnline());

            if (onlineMembers.Any())
            {
                var connections = (await Psijic.Once(onlineMembers
                    .Select(u => u.User.Connections.Value())))
                    .SelectMany(c => c)
                    .ToArray();

                await Terminal.SocketService.BroadcastAsync(client => client.UserIsComposing(user.Id, Id, isComposing), connections);
            }
        }

        #endregion

        #region Dissimilation

        public override bool Equals(object obj)
		{
			return obj is Conversation other &&
                Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		#endregion
	}
}

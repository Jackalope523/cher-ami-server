using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Core.Boundaries
{
    #region Schemas

	public enum ChatType
	{
		Individual,
		Group,
		Unit,
		Broadcast,
	}

	public enum MembershipType
	{
		Regular,
		Owner,
	}

	public enum MessageType
	{
		Activity,
		Text,
		Photo,
		Segment,
		Post,
		Profile,
	}

	public enum ActivityMessageType
	{
        Initiated = 0, Edited = 1,
        Joined = 10, Left = 11, Invited = 12, Kicked = 13,
    }

	public record ActivityMessageShard(ActivityMessageType Activity, long? ActorId = null, long? TargetId = null, string Info = null);

    public record CoreConversation(long Id, ChatType Type, DateTimeOffset DateCreated, string Title = default, long? GroupId = null)
		: CoreOnlyData();
	public record ConversationShard(long Id, ChatType Type, int LastPage, string Title = default,
		long? GroupId = null, bool? Muted = null, int? Unread = null);

	public record CoreMembership(long UserId, MembershipType Type, DateTimeOffset LastSeen, bool Muted)
		: CoreOnlyData();
	public record MembershipShard(long UserId, MembershipType Type, DateTimeOffset LastSeen);

	public record MessageShard(long Id, long UserId, DateTimeOffset Timestamp, MessageType Type, object Value);

    #endregion

    #region Gates

    public interface IMessageDatabase
    {
		Task<CoreConversation> GetConversationAsync(long conversationId);
		Task<int> GetLastPageNumber(long conversationId);

		Task<bool> IndividualConversationBetweenExists(long userIdA, long userIdB);
		Task<CoreConversation> GetOrCreateIndividualConversationBetween(long userIdA, long userIdB, DateTimeOffset currentTime);

		Task<bool> GroupConversationExists(long groupId);
		Task<CoreConversation> GetOrCreateGroupConversation(long groupId, DateTimeOffset currentTime);

		Task<long> CreateGroupChatConversationAsync(DateTimeOffset currentTime, string title = default);

		Task<List<CoreConversation>> GetConversationsForUserAsync(long userId);
		Task<List<CoreMembership>> GetConversationMembersAsync(long conversationId);
		Task<CoreMembership> GetMembershipAsync(long conversationId, long userId);

		Task UpdateConversationAsync(long conversationId, List<(string Property, object Value)> edits);
		Task DeleteConversationAsync(long conversationId);

		Task AddUsersToConversationAsync(long conversationId, params long[] userIds);
		Task UpdateMembershipAsync(long conversationId, long userId, List<(string Property, object Value)> edits);
		Task RemoveUserFromConversationAsync(long conversationId, long userId);

		Task<List<MessageShard>> GetMessagesForConversationAsync(long conversationId, int pageNumber);
		Task<int> GetMessageCountSinceAsync(long conversationId, DateTimeOffset timestamp);
        Task<MessageShard> AddMessageAsync(long conversationId, long userId, DateTimeOffset timestamp, MessageType type, object value);
    }

	public interface IMessageOperations
	{
		Task<List<ConversationShard>> GetConversationsAsync(long userId);
		Task<ConversationShard> GetAnnouncementsAsync(long userId, string locale);

		Task<ConversationShard> GetConversationWithAsync(long userId, long targetId);
		Task<ConversationShard> GetOrCreateConversationWithAsync(long userId, long targetId);
		Task<ConversationShard> GetGroupConversationAsync(long userId, long groupId);
		Task<ConversationShard> GetOrCreateGroupConversationAsync(long userId, long groupId);

		Task<ConversationShard> GetConversationAsync(long userId, long conversationId);
		Task<List<MembershipShard>> GetMembersAsync(long userId, long conversationId);
		Task<List<MessageShard>> GetMessagesAsync(long userId, long conversationId, int pageNumber);

		Task UserReadAsync(long userId, long conversationId);
		Task UserComposingAsync(long userId, long conversationId, bool isComposing);
		Task<MessageShard> SendTextAsync(long userId, long conversationId, string text);
		Task<MessageShard> SendPhotoAsync(long userId, long conversationId, MemoryStream photo);

		Task<MessageShard[]> ShareSegmentAsync(long userId, long conversationId, long[] segmentIds);
		Task<MessageShard[]> SharePostAsync(long userId, long conversationId, long[] postIds);
		Task<MessageShard[]> ShareProfileAsync(long userId, long conversationId, long[] profileIds);

		Task<ConversationShard> CreateGroupChatAsync(long userId, params long[] participantIds);
		Task EditGroupChatAsync(long userId, long conversationId,
			string title = "", MemoryStream header = null);
		Task DeleteGroupChatAsync(long userId, long conversationId);

		Task LeaveGroupChatAsync(long userId, long conversationId);
		Task InviteUserAsync(long userId, long conversationId, long targetId);
		Task KickUserAsync(long userId, long conversationId, long targetId);
	}

	public interface IMessageSocket
	{
		Task ReceiveMessage(long conversationId, MessageShard message);
		Task ReceiveMessages(long conversationId, MessageShard[] messages);
		Task UserIsComposing(long userId, long conversationId, bool isComposing);
	}

	#endregion
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Core.Boundaries
{
    #region Schemas

	public enum ChatType
	{
		Group,
		Individual,
		Broadcast,
	}

	public enum ChatMembershipType
	{ Regular, Owner }

	public enum MessageType
	{
		Activity,
		Text,
		Photo,
		Issue,
		Post,
		Profile,
	}

	public enum ActivityMessageType
	{
        Initiated = 0, Edited = 1,
        Joined = 10, Left = 11, Invited = 12, Kicked = 13,
    }

	public record ActivityMessageShard(ActivityMessageType Activity, long? ActorId = null, long? TargetId = null, string Info = null);

    public record CoreChat(long Id, ChatType Type, DateTimeOffset DateCreated, string Title = default, long? GroupId = null)
		: CoreOnlyData();
	public record ChatShard(long Id, ChatType Type, int LastPage, string Title = default,
		long? GroupId = null, bool? Muted = null, int? Unread = null);

	public record CoreMembership(long UserId, ChatMembershipType Type, DateTimeOffset LastSeen, bool Muted)
		: CoreOnlyData();
	public record MembershipShard(long UserId, ChatMembershipType Type, DateTimeOffset LastSeen);

	public record MessageShard(long Id, long UserId, DateTimeOffset Timestamp, MessageType Type, object Value);
	
    #endregion

    #region Gates

    public interface IChatDatabase
    {
		Task<CoreChat> GetChatAsync(long chatId);
		Task<int> GetLastPageNumber(long chatId);

		Task<bool> IndividualChatBetweenExists(long userIdA, long userIdB);
		Task<CoreChat> GetOrCreateIndividualChatBetween(long userIdA, long userIdB, DateTimeOffset currentTime);

		Task<bool> GroupChatExists(long groupId);
		Task<CoreChat> GetOrCreateGroupChat(long groupId, DateTimeOffset currentTime);

		Task<List<CoreChat>> GetChatsForUserAsync(long userId);
		Task<List<CoreMembership>> GetChatMembersAsync(long chatId);
		Task<CoreMembership> GetMembershipAsync(long chatId, long userId);

		Task UpdateChatAsync(long chatId, List<(string Property, object Value)> edits);
		Task DeleteChatAsync(long chatId);

		Task AddUsersToChatAsync(long chatId, params long[] userIds);
		Task UpdateMembershipAsync(long chatId, long userId, List<(string Property, object Value)> edits);
		Task RemoveUserFromChatAsync(long chatId, long userId);

		Task<List<MessageShard>> GetMessagesForChatAsync(long chatId, int pageNumber);
		Task<int> GetMessageCountSinceAsync(long chatId, DateTimeOffset timestamp);
        Task<MessageShard> AddMessageAsync(long chatId, long userId, DateTimeOffset timestamp, MessageType type, object value);
    }

	public interface IChatOperations
	{
		Task<List<ChatShard>> GetChatsAsync(long userId);
		Task<ChatShard> GetAnnouncementsAsync(long userId, string locale);

		Task<ChatShard> GetChatWithAsync(long userId, long targetId);
		Task<ChatShard> GetOrCreateChatWithAsync(long userId, long targetId);
		Task<ChatShard> GetGroupChatAsync(long userId, long groupId);

		Task<ChatShard> GetChatAsync(long userId, long chatId);
		Task<List<MembershipShard>> GetMembersAsync(long userId, long chatId);
		Task<List<MessageShard>> GetMessagesAsync(long userId, long chatId, int pageNumber);

		Task UserReadAsync(long userId, long chatId);
		Task UserComposingAsync(long userId, long chatId, bool isComposing);

		Task<MessageShard> SendTextAsync(long userId, long chatId, string text);
		Task<MessageShard> SendPhotoAsync(long userId, long chatId, MemoryStream photo);

		Task<MessageShard[]> ShareIssueAsync(long userId, long chatId, long[] issueIds);
		Task<MessageShard[]> SharePostAsync(long userId, long chatId, long[] postIds);
		Task<MessageShard[]> ShareProfileAsync(long userId, long chatId, long[] profileIds);
	}

	public interface IMessageSocket
	{
		Task ReceiveMessage(long chatId, MessageShard message);
		Task ReceiveMessages(long chatId, MessageShard[] messages);
		Task UserIsComposing(long userId, long chatId, bool isComposing);
	}

	#endregion
}

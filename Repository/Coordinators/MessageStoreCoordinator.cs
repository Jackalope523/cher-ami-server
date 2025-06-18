using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Repository
{
    class MessageStoreCoordinator : IChatDatabase
    {
        private readonly IChatDatabase store;

        public MessageStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFMessageStore(flag);
        }

        public Task<MessageShard> AddMessageAsync(long chatId, long userId, DateTimeOffset timestamp, MessageType type, object value)
        {
            return store.AddMessageAsync(conversationId, userId, timestamp, type, value);
        }

        public Task AddUsersToChatAsync(long chatId, params long[] userIds)
        {
            return store.AddUsersToConversationAsync(conversationId, userIds);
        }

        public Task<long> CreateGroupChatConversationAsync(DateTimeOffset currentTime, string title = null)
        {
            return store.CreateGroupChatConversationAsync(currentTime, title);
        }

        public Task DeleteChatAsync(long chatId)
        {
            return store.DeleteConversationAsync(conversationId);
        }

        public Task<bool> GroupChatExists(long gatheringId)
        {
            return store.GroupChatExists(gatheringId);
        }

        public Task<CoreChat> GetChatAsync(long chatId)
        {
            return store.GetChatAsync(conversationId);
        }

        public Task<List<CoreMembership>> GetChatMembersAsync(long chatId)
        {
            return store.GetConversationMembersAsync(conversationId);
        }

        public Task<int> GetLastPageNumber(long chatId)
        {
            return store.GetLastPageNumber(conversationId);
        }

        public Task<List<CoreChat>> GetChatsForUserAsync(long userId)
        {
            return store.GetChatsForUserAsync(userId);
        }

        public Task<CoreMembership> GetMembershipAsync(long chatId, long userId)
        {
            return store.GetMembershipAsync(conversationId, userId);
        }

        public Task<List<MessageShard>> GetMessagesForChatAsync(long chatId, int pageNumber)
        {
            return store.GetMessagesForConversationAsync(conversationId, pageNumber);
        }

        public Task<CoreChat> GetOrCreateGroupChat(long gatheringId, DateTimeOffset currentTime)
        {
            return store.GetOrCreateGroupChat(gatheringId, currentTime);
        }

        public Task<CoreChat> GetOrCreateIndividualChatBetween(long userIdA, long userIdB, DateTimeOffset currentTime)
        {
            return store.GetOrCreateIndividualChatBetween(userIdA, userIdB, currentTime);
        }

        public Task<bool> IndividualChatBetweenExists(long userIdA, long userIdB)
        {
            return store.IndividualChatBetweenExists(userIdA, userIdB);
        }

        public Task RemoveUserFromChatAsync(long chatId, long userId)
        {
            return store.RemoveUserFromConversationAsync(conversationId, userId);
        }

        public Task UpdateChatAsync(long chatId, List<(string Property, object Value)> edits)
        {
            return store.UpdateConversationAsync(conversationId, edits);
        }

        public Task UpdateMembershipAsync(long chatId, long userId, List<(string Property, object Value)> edits)
        {
            return store.UpdateMembershipAsync(conversationId, userId, edits);
        }

        public Task<int> GetMessageCountSinceAsync(long chatId, DateTimeOffset timestamp)
        {
            return store.GetMessageCountSinceAsync(conversationId, timestamp);
        }
    }
}

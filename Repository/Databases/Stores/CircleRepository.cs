using Repository.Databases.Contexts;

namespace Repository.Databases.Stores
{
    public class CircleRepository : Repository, ICircleDatabase
    {
        internal CircleRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {

        }

        public Task<CoreCircle> GetCircleAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<CoreCircle> GetCircleByCodeAsync(string circleCode)
        {
            throw new NotImplementedException();
        }

        public Task<List<CoreCircle>> GetCirclesForUserAsync(long userId)
        {
            throw new NotImplementedException();
        }

        public Task<CoreCircle> CreateCircleAsync(long ownerId, string title, CirclePlan plan, IssueSchedule schedule)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCircleAsync(long circleId, List<(string Property, object Value)> edits)
        {
            throw new NotImplementedException();
        }

        public Task<string> RerollCircleCode(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCircleAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CoreCircleMembership>> GetCircleMembersAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<List<RecipientShard>> GetRecipientsForCircleAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<CoreCircleMembership> GetCircleMembershipAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCircleMemberAsync(long userId, long circleId, List<(string Property, object Value)> edits)
        {
            throw new NotImplementedException();
        }

        public Task AddCircleMemberAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCircleMemberAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task AddRecipientAsync(long circleId, long userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRecipientAsync(long recipientId)
        {
            throw new NotImplementedException();
        }

        public Task SoftDeleteAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task HardDeleteAsync(long circleId)
        {
            throw new NotImplementedException();
        }
    }
}


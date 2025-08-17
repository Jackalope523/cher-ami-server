using Core.Boundaries;
using LazyLizardBackend.Contracts.Responses;
using LazyLizardBackend.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LazyLizardBackend.Services
{
    public class CircleService(ICircleRepository circleRepository, IMediaRepository mediaRepository) : ICircleService
	{
        public async Task<List<CoreCircle>> GetUserCirclesAsync(long userId)
        {
            return await circleRepository.GetCirclesForUserAsync(userId);
        }

        public async Task<CoreCircle> GetCircleInformationAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");

            return await circleRepository.GetCircleAsync(circleId);
        }

        public async Task<CoreCircle> CreateCircleAsync(long userId, string title, CirclePlan plan, IssueSchedule schedule, MemoryStream header)
        {
            CoreCircle toReturn = await circleRepository.CreateCircleAsync(userId, title, plan, schedule);
            await mediaRepository.UploadCircleHeaderAsync(toReturn.Id, header);

            return toReturn;  
        }

        public async Task EditCircleAsync(long userId, long circleId, string title = "", CirclePlan? plan = null, IssueSchedule? schedule = null, MemoryStream header = null)
        {
            if (!await circleRepository.IsMemberOfTypeAsync(userId, circleId, CircleMembershipType.Owner))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");

            List<(string, object)> edits = 
                [
                (nameof(CoreCircle.Title), title), 
                (nameof(CoreCircle.Plan), plan),
                (nameof(CoreCircle.Schedule), schedule),
                ("Header", header),
                ];

            await circleRepository.UpdateCircleAsync(circleId, edits);
        }

        public async Task<string> RerollCircleCodeAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberOfTypeAsync(userId, circleId, CircleMembershipType.Owner))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");

            return await circleRepository.RerollCircleCode(circleId);
        }

        public async Task DeleteCircleAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberOfTypeAsync(userId, circleId, CircleMembershipType.Owner))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");

            await circleRepository.DeleteCircleAsync(circleId);
        }

        public async Task<List<CoreCircleMembership>> GetCircleMembers(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");

            return await circleRepository.GetCircleMembersAsync(circleId);
        }

        public async Task JoinCircleAsync(long userId, string circleCode)
        {
            await circleRepository.AddCircleMemberAsync(userId, circleCode);
        }

        public async Task RemoveMemberAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");

            await circleRepository.RemoveCircleMembershipAsync(userId, circleId);
        }

        public async Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");
            
            return await circleRepository.GetRecipientsForCircleAsync(circleId);
        }

        public async Task EditRecipientAsync(long userId, long recipientId, List<(string Property, object Value)> edits)
        {
            if (!await circleRepository.IsManagerAsync(userId, recipientId))
                throw new NoAccessException($"User {userId} does not have access to recipient {recipientId}.");

            await circleRepository.UpdateRecipientAsync(recipientId, edits);
        }

        public async Task AddRecipientAsync(long userId, long circleId, long recipientId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");

            await circleRepository.AddRecipientAsync(circleId, recipientId);
        }

        public async Task RemoveRecipientAsync(long userId, long circleId, long recipientId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} does not have access to circle {circleId}.");

            await circleRepository.RemoveRecipientAsync(circleId, recipientId);
        }

        public async Task CreateRecipient(CoreRecipient recipient)
        {
            await circleRepository.CreateRecipient(recipient);
        }

        public async Task DeleteRecipientAsync(long userId,long recipientId)
        {
            if (!await circleRepository.IsManagerAsync(userId, recipientId))
                throw new NoAccessException($"User {userId} does not have access to recipient {recipientId}.");

            await circleRepository.DeleteRecipientAsync(recipientId);
        }
    }
}

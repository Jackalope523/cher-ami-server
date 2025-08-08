using Core.Boundaries;
using LazyLizardBackend.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Services
{
    public class CircleService(ICircleRepository circleRepository, IMediaRepository mediaRepository) : ICircleService
	{
        public async Task<List<CoreCircle>> GetUserCirclesAsync(long userId)
        {
            return await circleRepository.GetCirclesForUserAsync(userId);
        }

        public Task<CoreCircle> GetCircleInformationAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public async Task<CoreCircle> CreateCircleAsync(long userId, string title, CirclePlan plan, IssueSchedule schedule, MemoryStream header)
        {
            CoreCircle toReturn = await circleRepository.CreateCircleAsync(userId, title, plan, schedule);
            await mediaRepository.UploadCircleHeaderAsync(toReturn.Id, header);

            return toReturn;  
        }

        public Task EditCircleAsync(long userId, long circleId, string title = "", CirclePlan? plan = null, IssueSchedule? schedule = null, MemoryStream header = null)
        {
            throw new NotImplementedException();
        }

        public async Task<string> RerollCircleCodeAsync(long userId, long circleId)
        {
            return await circleRepository.RerollCircleCode(circleId);
        }

        public async Task DeleteCircleAsync(long userId, long circleId)
        {
            await circleRepository.DeleteCircleAsync(circleId);
        }

        public async Task<List<CoreCircleMembership>> GetCircleMembers(long userId, long circleId)
        {
            return await circleRepository.GetCircleMembersAsync(circleId);
        }

        public Task SendInvitationAsync(long userId, long circleId, string phoneNumber = null, string email = null)
        {
            throw new NotImplementedException();
        }

        public async Task JoinCircleAsync(long userId, string circleCode)
        {
            await circleRepository.AddCircleMemberAsync(userId, circleCode);
        }

        public async Task RemoveMemberAsync(long userId, long circleId)
        {
            await circleRepository.RemoveCircleMembershipAsync(userId, circleId);
        }

        public async Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long userId, long circleId)
        {
            return await circleRepository.GetRecipientsForCircleAsync(circleId);
        }

        public Task EditRecipientAsync(long recipientId, List<(string Property, object Value)> edits)
        {
            throw new NotImplementedException();
        }

        public async Task AddRecipientAsync(long circleId, long recipientId)
        {
            await circleRepository.AddRecipientAsync(circleId, recipientId);
        }

        public async Task RemoveRecipientAsync(long circleId, long recipientId)
        {
            await circleRepository.RemoveRecipientAsync(circleId, recipientId);
        }

        public async Task CreateRecipient(CoreRecipient recipient)
        {
            await circleRepository.CreateRecipient(recipient);
        }

        public async Task DeleteRecipientAsync(long recipientId)
        {
            await circleRepository.DeleteRecipientAsync(recipientId);
        }
    }
}

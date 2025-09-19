using Core.Boundaries;
using CrazyLizard.Exceptions;
using CrazyLizard.Entities;
using Stripe;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CrazyLizard.Boundaries.Repository;

namespace CrazyLizard.Services
{
    public class CircleService(ICircleRepository circleRepository, IAccountRepository accountRepository, IMediaRepository mediaRepository, StripeClient stripeClient) : ICircleService
	{
        public async Task<CoreCircle> GetCircleForUserAsync(long userId)
        {
            return await circleRepository.GetCircleForUserAsync(userId);
        }

        public async Task<CoreCircle> GetCircleInformationAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} is not a member of circle {circleId}.");

            return await circleRepository.GetCircleAsync(circleId);
        }

        public async Task<CoreCircle> CreateCircleAsync(long userId, string title, IssueSchedule schedule, MemoryStream header)
        {
            if (await circleRepository.HasCircle(userId))
                throw new ValidationException($"User {userId} is already part of a circle.");

            CoreCircle toReturn = await circleRepository.CreateCircleAsync(userId, title, schedule);
            await mediaRepository.UploadCircleHeaderAsync(toReturn.Id, header);

            return toReturn;  
        }

        public async Task EditCircleAsync(long userId, long circleId, string title = "", IssueSchedule? schedule = null, MemoryStream header = null)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} is not an admin of circle {circleId}.");

            List<(string, object)> edits = 
                [
                (nameof(CoreCircle.Title), title), 
                (nameof(CoreCircle.Schedule), schedule),
                ("Header", header),
                ];

            await circleRepository.UpdateCircleAsync(circleId, edits);
        }

        public async Task<string> RerollCircleCodeAsync(long userId, long circleId)
        {
            if (!await circleRepository.Exists(circleId))
                throw new NotFoundException($"Circle {circleId} does not exist.");

            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} is not a member of circle {circleId}.");

            return await circleRepository.RerollCircleCode(circleId);
        }

        public async Task DeleteCircleAsync(long userId, long circleId)
        {
            if (!await circleRepository.Exists(circleId))
                throw new NotFoundException($"Circle {circleId} does not exist.");

            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NotFoundException($"User {userId} is not a member of circle {circleId}.");

            await circleRepository.DeleteCircleAsync(circleId);
        }

        public async Task<List<User>> GetCircleMembers(long userId)
        {
            CoreCircle circle = await circleRepository.GetCircleForUserAsync(userId);

            if (circle == null)
                throw new NotFoundException($"Circle {circle.Id} does not exist.");
            
            return await circleRepository.GetCircleContributorsAsync(circle.Id);
        }

        public async Task AddMemberAsync(long userId, string circleCode)
        {
            if (!await circleRepository.Exists(circleCode))
                throw new NotFoundException($"Circle with code \"{circleCode}\" does not exist.");

            if (await circleRepository.HasCircle(userId))
                throw new ValidationException($"User {userId} is already part of a circle.");

            await circleRepository.AddCircleMemberAsync(userId, circleCode);
        }

        public async Task RemoveMemberAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NotFoundException($"User {userId} is not a member of circle {circleId}.");

            await circleRepository.RemoveCircleMembershipAsync(userId);
        }

        public async Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long userId)
        {
            CoreCircle circle = await circleRepository.GetCircleForUserAsync(userId);

            if (circle == null)
                throw new NotFoundException($"Could not find circle for user {userId}");
            
            return await circleRepository.GetRecipientsForCircleAsync(circle.Id);
        }
    }
}

using CrazyLizard.Exceptions;
using CrazyLizard.Entities;
using Stripe;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CrazyLizard.Boundaries.Repository;
using CrazyLizard.Interfaces.Repository;
using CrazyLizard.Interfaces.Service;

namespace CrazyLizard.Services
{
    public class CircleService(ICircleRepository circleRepository, IMediaRepository mediaRepository) : ICircleService
	{
        public async Task<Circle> GetCircleForUserAsync(long userId)
        {
            return await circleRepository.GetCircleForUserAsync(userId);
        }

        public async Task<Circle> GetCircleInformationAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} is not a member of circle {circleId}.");

            return await circleRepository.GetCircleAsync(circleId);
        }

        public async Task<Circle> CreateCircleAsync(long userId, string title, IssueSchedule schedule, MemoryStream header)
        {
            if (await circleRepository.HasCircle(userId))
                throw new ValidationException($"User {userId} is already part of a circle.");

            Circle toReturn = await circleRepository.CreateCircleAsync(userId, title, schedule);
            await mediaRepository.UploadCircleHeaderAsync(toReturn.Id, header);

            return toReturn;  
        }

        public async Task EditCircleAsync(long userId, long circleId, string title = "", IssueSchedule? schedule = null, MemoryStream header = null)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} is not an admin of circle {circleId}.");

            List<(string, object)> edits = 
                [
                (nameof(Circle.Title), title), 
                (nameof(Circle.IssueSchedule), schedule),
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
            Circle circle = await circleRepository.GetCircleForUserAsync(userId);

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

        public async Task<List<Recipient>> GetRecipientsForCircleAsync(long userId)
        {
            Circle circle = await circleRepository.GetCircleForUserAsync(userId);

            if (circle == null)
                throw new NotFoundException($"Could not find circle for user {userId}");
            
            return await circleRepository.GetRecipientsForCircleAsync(circle.Id);
        }
    }
}

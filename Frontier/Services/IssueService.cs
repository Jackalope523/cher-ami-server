using Core.Boundaries;
using CrazyLizard.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CrazyLizard.Services
{
    public class IssueService(IIssueRepository issueRepository, ICircleRepository circleRepository) : IIssueService
    {
        public async Task<CorePost> AddPostAsync(long userId, long issueId, DateTimeOffset timestamp, string caption, MemoryStream image)
        {
            if (!await issueRepository.Exists(issueId))
                throw new ValidationException($"Issue {issueId} does not exist.");

            return await issueRepository.AddPostAsync(issueId, userId, timestamp, caption, image);
        }

        public async Task DeletePostAsync(long userId, long postId)
        {
            if (!await issueRepository.IsOwner(postId, postId))
                throw new NoAccessException($"User {userId} does not own post {postId}");

            if (!await issueRepository.IsDraft(postId, DateTimeOffset.UtcNow))
                throw new DeleteException($"Post {postId} is no longer in drafting.");

            await issueRepository.DeletePostAsync(postId);
        }

        public Task EditPostAsync(long userId, long postId, DateTimeOffset? timestamp = null, string caption = null, MemoryStream image = null)
        {
            throw new NotImplementedException();
        }

        public async Task<CoreIssue> GetIssueAsync(long userId, long issueId)
        {
            if (!await issueRepository.IsContributor(userId, issueId))
                throw new NoAccessException($"User {userId} is not a contributor to issue {issueId}.");

            return await issueRepository.GetIssueAsync(issueId);
        }

        public async Task<List<CoreIssue>> GetIssuesForCircleAsync(long userId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(userId, circleId))
                throw new NoAccessException($"User {userId} can not access issues of circle {circleId}.");

            return await issueRepository.GetIssuesForCircleAsync(circleId);
        }

        public async Task<CorePost> GetPostAsync(long userId, long postId)
        {
            if (!await issueRepository.IsContributorToIssueOf(userId, postId))
                throw new NoAccessException($"User {userId} is not a contributor to issue of post {postId}.");

            return await issueRepository.GetPostAsync(postId);
        }

        public async Task<List<CorePost>> GetPostsForIssueAsync(long userId, long issueId)
        {
            if (!await issueRepository.IsContributor(userId, issueId))
                throw new NoAccessException($"User {userId} is not a contributor to issue {issueId}.");

            return await issueRepository.GetPostsForIssueAsync(issueId);
        }
    }
}


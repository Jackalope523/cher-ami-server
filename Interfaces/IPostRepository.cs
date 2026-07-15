using CherAmiAPI.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IPostRepository
    {
        Task<Post> GetPostAsync(long postId, CancellationToken cancellationToken = default);
        Task<Post> GetPostByUploadIdAsync(string uploadId, CancellationToken cancellationToken = default);
        Task AddPostAsync(Post post, CancellationToken cancellationToken = default);
        Task SavePostAsync(Post post, CancellationToken cancellationToken = default);
        Task RemovePostAsync(Post post, CancellationToken cancellationToken = default);
        Task<bool> IsAuthorAsync(long postId, long userId, CancellationToken cancellationToken = default);
        Task CreatePostReportAsync(long postId, long filingUserId, CancellationToken cancellationToken = default);
        Task<long> GetCurrentIssueIdAsync(long circleId, CancellationToken cancellationToken = default);
        Task<long> GetFirstIssueIdOfCircleAsync(long circleId, CancellationToken cancellationToken = default);
        Task<Issue> GetFeedPageAsync(long circleId, int page, List<long> excludedAuthorIds, CancellationToken cancellationToken = default);
        Task<int> CountIssuesOfCircleAsync(long circleId, CancellationToken cancellationToken = default);
        Task<int> GetLatestIssuePostCountAsync(long circleId, CancellationToken cancellationToken = default);
        Task<long> GetCircleIdOfPostAsync(long postId, CancellationToken cancellationToken = default);
        Task<string> GetLowResolutionImagePathAsync(long postId, CancellationToken cancellationToken = default);
        Task<List<string>> GetImagePathsByAuthorAsync(long authorId, CancellationToken cancellationToken = default);
    }
}

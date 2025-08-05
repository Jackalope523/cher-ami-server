using System;
using System.Net;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public record CoreOnlyData();

    public record ErrorShard(HttpStatusCode Code, string Details);

    #endregion

    #region Gates

    public interface IMiscellaneousRepository
    {
		Task SaveFeedbackAsync(string comments, DateTimeOffset time);
        Task SaveFeedbackAsync(string comments, DateTimeOffset time, long userId);
    }

    public interface IMiscellaneousOperations
    {
		Task ReceiveFeedback(long userId, string comments);
		Task ReceiveAnonymousFeedback(long userId, string comments);
	}

    #endregion
}


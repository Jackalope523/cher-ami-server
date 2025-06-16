using System;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public record ErrorShard(string Code, object Details = null);

    #endregion

    #region Gates

    public interface IMiscellaneousDatabase
    {
		Task SaveFeedbackAsync(string comments, DateTimeOffset time);
        Task SaveFeedbackAsync(string comments, DateTimeOffset time, long userId);
    }

    public interface IMiscellaneousOperations
    {
		Task ReceiveFeedback(long userId, string comments);
		Task ReceiveAnonymousFeedback(long userId, string pseudonym, string comments);
	}

    #endregion
}


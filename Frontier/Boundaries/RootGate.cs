using System;
using System.Net;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    public interface IMiscellaneousRepository
    {
		Task SaveFeedbackAsync(string comments, DateTimeOffset time);
        Task SaveFeedbackAsync(string comments, DateTimeOffset time, long userId);
    }

    public interface IMiscellaneousService
    {
		Task ReceiveFeedback(long userId, string comments);
		Task ReceiveFeedback(string comments);
	}
}


using System;
using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Repository
{
    public interface IMiscellaneousRepository
    {
		Task SaveFeedbackAsync(string comments, DateTimeOffset time);
        Task SaveFeedbackAsync(string comments, DateTimeOffset time, long userId);
    }
}


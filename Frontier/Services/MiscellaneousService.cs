using System;
using System.Threading.Tasks;

namespace LazyLizardBackend.Services
{
    public class MiscellaneousService(IMiscellaneousRepository miscellaneousRepository) : IMiscellaneousService
    {
        public async Task ReceiveFeedback(long userId, string comments)
        {
            await miscellaneousRepository.SaveFeedbackAsync(comments, DateTimeOffset.UtcNow, userId);
        }

        public async Task ReceiveFeedback(string comments)
        {
            await miscellaneousRepository.SaveFeedbackAsync(comments, DateTimeOffset.UtcNow);
        }
    }
}

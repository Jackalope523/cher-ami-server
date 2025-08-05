using System.IO;
using System.Threading.Tasks;
using Core.Boundaries;
using static LazyLizardBackend.Psijic;

namespace Core.Services
{
    public class MiscellaneousService(IMiscellaneousRepository miscellaneousRepository) : IMiscellaneousService
    {
        public async Task ReceiveFeedback(long userId, string comments)
        {
            await miscellaneousRepository.SaveFeedbackAsync(comments, Time, userId);
        }

        public async Task ReceiveFeedback(string comments)
        {
            await miscellaneousRepository.SaveFeedbackAsync(comments, Time);
        }
    }
}

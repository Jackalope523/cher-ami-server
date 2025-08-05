using System.IO;
using System.Threading.Tasks;
using Core.Boundaries;

using static Core.Entities.Psijic;

namespace Core.Services
{
    public class MiscellaneousService : AbstractService, IMiscellaneousOperations
    {
		#region Initialisation

		public MiscellaneousService(CoreTerminal terminal) : base(terminal) { }

        #endregion

        #region Operations

        public async Task ReceiveFeedback(long userId, string comments)
        {
            var user = await GetUserAsync(userId);

            await Miscellaneous.SaveFeedbackAsync(comments, Time, user.Id);
        }

        public async Task ReceiveAnonymousFeedback(long userId, string comments)
        {
            await GetUserAsync(userId); // Just to verify account

            await Miscellaneous.SaveFeedbackAsync(comments, Time);
        }

        #endregion

        #region Favours


        #endregion
    }
}

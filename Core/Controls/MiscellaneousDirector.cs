using System.IO;
using System.Threading.Tasks;
using Core.Boundaries;

using static Core.Entities.Psijic;

namespace Core.Controls
{
    internal class MiscellaneousDirector : AbstractDirector, IMiscellaneousOperations
    {
		#region Initialisation

		public MiscellaneousDirector(CoreTerminal terminal) : base(terminal) { }

        #endregion

        #region Operations

        public async Task ReceiveFeedback(long userId, string comments)
        {
            var user = await GetUserAsync(userId);

            await Miscellaneous.SaveFeedbackAsync(comments, Time, user.Id);
        }

        public async Task ReceiveAnonymousFeedback(long userId, string pseudonym, string comments)
        {
            await GetUserAsync(userId); // Just to verify account, promise :)

            await Miscellaneous.SaveFeedbackAsync(comments, Time);
        }

        #endregion

        #region Favours


        #endregion
    }
}

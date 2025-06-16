using System.IO;
using System.Threading.Tasks;
using Core.Boundaries;

using static Core.Entities.Arbiter;

namespace Core.Controls
{
    internal class KeyDirector : AbstractDirector, IKeyOperations
	{
		#region Initialisation

		public KeyDirector(CoreTerminal terminal) : base(terminal) { }

        #endregion

        #region Operations

        public async Task<string> GetCanaryMapKeyAsync(long userId)
        {
            await GetUserAsync(userId);

            return await Keys.GetCanaryMapKeyAsync();
        }

        public async Task<string> GetClassifiedAccountCodeAsync(long userId)
        {
            return userId switch
            {
                -7 => await Keys.GetAppleAccountCodeAsync(),
                -8 => await Keys.GetGoogleAccountCodeAsync(),
                _ => throw new UndefinedBehaviourException($"Tried to access non-existent classified account code for {userId}")
            };
        }

        #endregion

        #region Favours


        #endregion
    }
}

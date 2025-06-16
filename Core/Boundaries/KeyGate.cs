using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    #endregion

    #region Gates

    public interface IKeyDatabase
    {
		Task<string> GetHollowOneSignalApiKeyAsync();
		Task<string> GetHollowOneSignalAppIdAsync();

		Task<string> GetHollowTwilioAccountKeyAsync();
		Task<string> GetHollowTwilioAuthTokenAsync();
		Task<string> GetHollowTwilioMessagingServiceAsync();

		Task<string> GetCanaryMapKeyAsync();

		Task<string> GetAppleAccountCodeAsync();
		Task<string> GetGoogleAccountCodeAsync();
    }

    public interface IKeyOperations
	{
		Task<string> GetCanaryMapKeyAsync(long userId);

		Task<string> GetClassifiedAccountCodeAsync(long userId);
	}

    #endregion
}


using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Repository
{
    public interface IKeyRepository
    {
		Task<string> GetHollowOneSignalApiKeyAsync();
		Task<string> GetHollowOneSignalAppIdAsync();

		Task<string> GetHollowTwilioAccountKeyAsync();
		Task<string> GetHollowTwilioAuthTokenAsync();
		Task<string> GetHollowTwilioMessagingServiceAsync();

		Task<string> GetAppleAccountCodeAsync();
		Task<string> GetGoogleAccountCodeAsync();
    }
}


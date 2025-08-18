using System.Threading.Tasks;
using Core.Boundaries;
using Frontier.Exceptions;

namespace LazyLizardBackend.Services
{
    public class KeyService(IKeyRepository keyRepository) : IKeyService
	{
        public async Task<string> GetClassifiedAccountCodeAsync(long userId)
        {
            return userId switch
            {
                -7 => await keyRepository.GetAppleAccountCodeAsync(),
                -8 => await keyRepository.GetGoogleAccountCodeAsync(),
                _ => throw new UndefinedBehaviourException($"Tried to access non-existent classified account code for {userId}")
            };
        }
    }
}

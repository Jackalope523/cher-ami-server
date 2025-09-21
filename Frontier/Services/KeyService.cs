using System.Threading.Tasks;
using CrazyLizard.Exceptions;
using CrazyLizard.Interfaces.Repository;
using CrazyLizard.Interfaces.Service;

namespace CrazyLizard.Services
{
    public class KeyService(IKeyRepository keyRepository) : IKeyService
	{
        public async Task<string> GetClassifiedAccountCodeAsync(long userId)
        {
            return userId switch
            {
                7 => await keyRepository.GetAppleAccountCodeAsync(),
                8 => await keyRepository.GetGoogleAccountCodeAsync(),
                _ => throw new NotFoundException($"Tried to access non-existent classified account code for {userId}")
            };
        }
    }
}

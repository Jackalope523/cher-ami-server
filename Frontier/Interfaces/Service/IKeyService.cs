using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Service
{
    public interface IKeyService
	{
        Task<string> GetSecretAsync(string name);
    }
}


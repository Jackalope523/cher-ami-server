using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IKeyService
	{
        Task<string> GetSecretAsync(string name);
    }
}


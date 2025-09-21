using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Service
{
    public interface IKeyService
	{
		Task<string> GetClassifiedAccountCodeAsync(long userId);
	}
}


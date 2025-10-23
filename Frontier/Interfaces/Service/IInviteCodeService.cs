using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces.Service
{
    public interface IInviteCodeService
	{
        Task<string> GenerateCodeAsync();
    }
}


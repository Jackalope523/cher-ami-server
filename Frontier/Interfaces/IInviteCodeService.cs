using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IInviteCodeService
	{
        Task<string> GenerateCodeAsync();
    }
}


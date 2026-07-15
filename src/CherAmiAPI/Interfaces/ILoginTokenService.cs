using CherAmiAPI.Entities;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface ILoginTokenService
    {
        Task<string> CreateLoginTokenAsync(User user);
    }
}

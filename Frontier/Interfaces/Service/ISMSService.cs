using System.Threading.Tasks;

namespace CrazyLizard.Boundaries.Service
{
	public interface ISMSService
	{
		Task SendTextMessageAsync(string phoneNumber, string message);
        Task SendWhatsAppAuthMessageAsync(string phoneNumber, string code);
    }
}

using System.Threading.Tasks;

namespace CrazyLizard.Boundaries.Service
{
    public interface IEmailService
	{
		Task SendEmailAsync(string email, string subject, string body);
	}
}

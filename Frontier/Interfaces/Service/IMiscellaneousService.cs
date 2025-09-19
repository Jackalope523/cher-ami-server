using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Service
{
    public interface IMiscellaneousService
    {
		Task ReceiveFeedback(long userId, string comments);
		Task ReceiveFeedback(string comments);
	}
}


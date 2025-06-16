using System;
using System.Threading.Tasks;
using SendGrid;
using Twilio.TwiML.Messaging;
using Twilio.Types;
using Core.Boundaries;

namespace Frontier.Services
{
	public class SendGridService : IEmailService
	{
		public static void Initialise(string accountId, string accountToken)
		{
			SendGridClient client = new(accountId, accountToken);
		}

		public Task SendEmailAsync(string email, string subject, string body)
		{
			Console.WriteLine($"Email to {email} [{subject}]: {body}");
			return Task.FromResult(0);
		}
	}
}

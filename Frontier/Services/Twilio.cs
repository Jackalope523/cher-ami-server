using System;
using System.Threading.Tasks;
using Twilio;
using Core;
using Twilio.Rest.Api.V2010.Account;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Frontier.Services
{
	public class TwilioService : ISMSService
	{
		private static ILogger log;
		private static EnvironmentOptions env;

		private static string messagingServiceSid = "";

		public static void Initialise(EnvironmentOptions environment, ILogger logger, string accountId, string accountToken, string messagingService)
		{
			env = environment;
			log = logger;
			messagingServiceSid = messagingService;

			TwilioClient.Init(accountId, accountToken);

            log.LogInformation("Twilio set up successfully.");
		}

		public async Task SendTextMessageAsync(string phoneNumber, string message)
        {
            string formattedPhoneNumber = $"+{phoneNumber}";

            log.LogInformation("Want to send SMS to {phoneNumber}", formattedPhoneNumber);

            if (env.IsProduction)
            {
                log.LogInformation("Sending SMS to {phoneNumber}: {message}", formattedPhoneNumber, message);

				await MessageResource.CreateAsync(
                    messagingServiceSid: messagingServiceSid,
                    to: new Twilio.Types.PhoneNumber(formattedPhoneNumber),
                    body: message);
            }
            else
            {
                log.LogInformation("Dropped SMS to {phoneNumber}: {message}", formattedPhoneNumber, message);
            }
        }

		public async Task SendWhatsAppAuthMessageAsync(string phoneNumber, string code)
        {
            phoneNumber = $"whatsapp:+{phoneNumber}";

            log.LogInformation("Want to send WhatsApp auth message to {phoneNumber}", phoneNumber);

            if (env.IsProduction)
            {
                log.LogInformation("Sending WhatsApp auth message to {phoneNumber}: {message}", phoneNumber, code);

				await MessageResource.CreateAsync(
                    messagingServiceSid: messagingServiceSid,
                    to: new Twilio.Types.PhoneNumber(phoneNumber),
                    contentSid: "HXa7108f0e6189b4b7fce12765ce15d7f6",
                    contentVariables: JsonConvert.SerializeObject(
                        new Dictionary<string, Object>() { { "1", code } }, Formatting.Indented));
            }
            else
            {
                log.LogInformation("Dropped WhatsApp auth message to {phoneNumber}: {message}", phoneNumber, code);
            }
        }
    }
}

using Core;
using Frontier.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Frontier.Controllers
{
    [Route("account")]
    public class AccountController : AbstractController
	{
        #region Initialisation

        SignInManager<CoreUser> signInManager;
        BypassHandler bypass;

        IEmailService emailService;
        ISMSService smsService;

		public AccountController(ControllerBox box, UserManager<CoreUser> aspUserManager,
            SignInManager<CoreUser> aspSignInManager,
            IEmailService externalEmailService, ISMSService externalSMSService) :
            base(box, aspUserManager)
		{
            signInManager = aspSignInManager;

            emailService = externalEmailService;
            smsService = externalSMSService;

            bypass = new(box.env, box.keys);
        }

		#endregion


        #region Tools

        private class BypassHandler
        {
            private EnvironmentOptions env;

            private string appleAccountCode;
            private string googleAccountCode;

            public BypassHandler(EnvironmentOptions environment, IKeyService keys)
            {
                env = environment;
                
                appleAccountCode = keys.GetClassifiedAccountCodeAsync(-7).Result;
                googleAccountCode = keys.GetClassifiedAccountCodeAsync(-8).Result;
            }

            public bool IsGlobalBypassEnabled()
            {
                return !env.IsProduction;
            }

            public bool IsClassifiedAccount(long userId)
            {
                return userId < 1;
            }

            public bool IsOperable(long userId)
            {
                return userId == -2 || userId == -7 || userId == -8;
            }

            public bool CheckStaticCode(long userId, string code)
            {
                if (!IsOperable(userId))
                { return false; }

                string staticCode = userId switch
                {
                    -2 => appleAccountCode,
                    -7 => appleAccountCode,
                    -8 => googleAccountCode,
                    _ => throw new UserErrorException(AccountErrorCode.NOT_FOUND)
                };

                return !string.IsNullOrEmpty(staticCode) && code.Equals(staticCode);
            }
        }

        #endregion
    }
}
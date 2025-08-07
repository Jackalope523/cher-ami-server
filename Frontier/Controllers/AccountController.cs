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

		#region Actions

        [HttpPost("email")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendEmailVerification(string email)
        {
            // Verify parameters
            if (string.IsNullOrEmpty(email))
			{ return MissingInformation(); }

            return await Execute(async () =>
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    throw new UserErrorException(AccountErrorCode.NOT_FOUND);
                }

                // Send verification email if email is not confirmed
                if (!user.IsEmailConfirmed)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("email", "account", new { token, email = user.Email }, Request.Scheme);
                    await emailService.SendEmailAsync(user.Email, "Verify your CANARY email.", $"Verify your CANARY email.\n\n{confirmationLink}");
                }
            });
		}

        [HttpPut]
        public async Task<IActionResult> ModifyAccount([FromForm] AccountEditManifest details)
        {
            // Verify parameters
			if (details == null)
			{ return MissingInformation(); }

            return await Execute(async user =>
            {
                await accounts.EditUserAsync(user.Id,
                    email: details.Email,
                    title: details.Title, givenName: details.GivenName, familyName: details.FamilyName,
                    dateOfBirth: details.DateOfBirth);
            });
        }

        [HttpGet("agreement")]
        public async Task<IActionResult> GetLastUserAgreement()
        {
            CoreUser user = await userManager.GetUserAsync(HttpContext.User);

            return Ok(user.TimeOfUserAgreement);
        }

        [HttpPost("agreement")]
        public async Task<IActionResult> UpdateUserAgreement()
        {
            CoreUser user = await userManager.GetUserAsync(HttpContext.User);

            await accounts.UpdateUserAgreementAsync(user.Id);
            return NoContent();
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> ModifyAvatar([FromForm] ImageManifest avatar)
        {
            // Verify parameters
            if (avatar == null || !ModelState.IsValid ||
                avatar.Image == null || avatar.Image.Length == 0)
            { return MissingInformation(); }

            return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                await avatar.Image.CopyToAsync(stream);

                await accounts.EditAvatarAsync(user.Id, stream);
            });
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
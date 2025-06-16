using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using System.IO;
using Core;

namespace Frontier.Controllers
{
    [Route("account")]
    public class AccountGuard : AbstractGuard
	{
        #region Initialisation

        SignInManager<CoreUser> signInManager;
        BypassHandler bypass;

        IEmailService emailService;
        ISMSService smsService;

		public AccountGuard(GuardBox box, UserManager<CoreUser> aspUserManager,
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

		[HttpGet]
        public async Task<IActionResult> GetAccount()
        {
            return await Execute(async user =>
                await accounts.GetAccountShardAsync(user.Id));
        }

		[HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(long userId)
        {
            return await Execute(async user =>
                await accounts.GetUserShardAsync(userId));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AccountCredentialsManifest credentials)
        {
            // Verify parameters
            if (credentials == null || !ModelState.IsValid)
            { return MissingInformation(); }

            return await Execute(async () =>
            {
                var user = await accounts.GetCoreUserAsync(credentials.PhoneNumber);

                #region UNSAFE — MODIFICATION AUTHORISATION FROM CHRONOS REQUIRED
                // Skip if bypass or classified
                if (bypass.IsGlobalBypassEnabled() ||
                    bypass.IsClassifiedAccount(user.Id))
                { return; }
                #endregion

                string code;

                // Verify that the account is activated
                if (await userManager.IsPhoneNumberConfirmedAsync(user))
                {
                    // Account is activated, generate regular 2FA token
                    code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
                }
                else
                {
                    // Account is not activated, generate change number token
                    code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                }

                bool useWhatsApp = credentials.UseWhatsApp ?? false;

                // Send user code
                if (useWhatsApp)
                {
                    await smsService.SendWhatsAppAuthMessageAsync(user.PhoneNumber, code);
                }
                else
                {
                    await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Canary code is {code}");
                }
            });
        }

        [HttpGet("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            return await Execute(async () =>
            {
                if (signInManager.IsSignedIn(HttpContext.User))
                {
                    await signInManager.SignOutAsync();
                }
            });
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode([FromBody] AccountCredentialsManifest credentials)
        {
            // Verify parameters
			if (credentials == null || !ModelState.IsValid || credentials.Code == null)
            { return MissingInformation(); }

            return await Execute(async () =>
            {
                var user = await accounts.GetCoreUserAsync(credentials.PhoneNumber);

                if (await userManager.IsLockedOutAsync(user))
				{
					throw new UserErrorException(AccountErrorCode.LOCKED_OUT);
                }

                #region UNSAFE — MODIFICATION AUTHORISATION FROM CHRONOS REQUIRED
                // Check if development environment or special account
                if (bypass.IsGlobalBypassEnabled())
                {
                    var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                    await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, code);
                    await signInManager.SignInAsync(user, false);
                    return;
                }
                else if(bypass.IsClassifiedAccount(user.Id))
                {
                    // Verify static code
                    if (bypass.IsGlobalBypassEnabled() || bypass.CheckStaticCode(user.Id, credentials.Code))
                    {
                        await signInManager.SignInAsync(user, false);
                        return;
                    }
                    else
                    { throw new UserErrorException(AccountErrorCode.INCORRECT_CODE); }
                }
                #endregion

                // Check if the account is activated
                if (await userManager.IsPhoneNumberConfirmedAsync(user))
                {
                    // Account is activated, check 2FA token validity
                    var result = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, credentials.Code);
                    if (result)
                    {
                        // Token matched, reset access tries and sign user in
                        await userManager.ResetAccessFailedCountAsync(user);
                        await signInManager.SignInAsync(user, false);
                    }
                    else
                    {
                        await userManager.AccessFailedAsync(user);
						throw new UserErrorException(AccountErrorCode.INCORRECT_CODE);
					}
                }
                else
                {
                    // Account is not activated, check change number token validity
                    var result = await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, credentials.Code);
                    if (result.Succeeded)
                    {
                        // Token matched, reset access tries and sign user in
                        await userManager.ResetAccessFailedCountAsync(user);
                        await signInManager.SignInAsync(user, false);

                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            // Send verification email if an email is added
                            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                            var confirmationLink = Url.Action("email", "account", new { token, email = user.Email }, Request.Scheme);
                            await emailService.SendEmailAsync(user.Email, "Welcome to CANARY!", $"Verify your CANARY email.\n\n{confirmationLink}");
                        }
                    }
                    else
                    {
                        await userManager.AccessFailedAsync(user);
                        throw new UserErrorException(AccountErrorCode.INCORRECT_CODE);
                    }
                }
            });
        }

        [HttpGet("email")]
        [AllowAnonymous]
		public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            // Verify parameters
			if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
			{ return MissingInformation(); }

            return await Execute(async () =>
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    throw new UserErrorException(AccountErrorCode.NOT_FOUND);
                }

                var result = await userManager.ConfirmEmailAsync(user, token);
            });
		}

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

		[HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAccount([FromBody] AccountSignUpManifest details)
		{
            // Verify parameters
			if (details == null || !ModelState.IsValid)
			{ return MissingInformation(); }

            return await Execute(async () =>
            {
                var userExists = await accounts.GetUserExistsAsync(details.PhoneNumber);

                if (!userExists)
                {
                    // Persist a new user
                    await accounts.CreateUserAsync(details.PhoneNumber, details.Email ?? "",
                        details.Name, details.DateOfBirth.ToUniversalTime());

                    // Send an SMS to new user with a generated change number token
                    var user = await accounts.GetCoreUserAsync(details.PhoneNumber);
                    var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                    await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Canary code is {code}");
                }
                else
                {
                    // Account already exists
                    var user = await accounts.GetCoreUserAsync(details.PhoneNumber);
                    string code;

                    // Login
                    if (await userManager.IsPhoneNumberConfirmedAsync(user))
                    {
                        code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
                    }
                    // Account is not activated, send an SMS with a generated change number token
                    else
                    {
                        code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                    }

                    await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Canary code is {code}");
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
                // Send updates to account manager
                await accounts.EditUserAsync(user.Id,
                    name: details.Name, email: details.Email);
            }, allowUnverified: true);
        }

        [HttpGet("agreement")]
        public async Task<IActionResult> GetLastUserAgreement()
        {
            return await Execute(user => Task.FromResult(user.TimeOfUserAgreement), allowUnverified: true);
        }

        [HttpPost("agreement")]
        public async Task<IActionResult> UpdateUserAgreement()
        {
            return await Execute(async user => await accounts.UpdateUserAgreementAsync(user.Id), allowUnverified: true);
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

                // Send avatar to account manager
                await accounts.EditAvatarAsync(user.Id, stream);
            }, allowUnverified: true);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            return await Execute(async user =>
            {
                await accounts.DeleteUserAsync(user.Id);
            }, allowUnverified: true);
        }

        #endregion

        #region Tools

        private class BypassHandler
        {
            private EnvironmentOptions env;

            private string appleAccountCode;
            private string googleAccountCode;

            public BypassHandler(EnvironmentOptions environment, IKeyOperations keys)
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
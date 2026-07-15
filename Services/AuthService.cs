using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using User = CherAmiAPI.Entities.User;

namespace CherAmiAPI.Services
{
    public class AuthService(
        UserManager<User> userManager,
        IUserRepository userRepository,
        IAuthRepository authRepository,
        OneSignalService oneSignalService,
        INameService nameService,
        CustomerService customerService,
        CircleService circleService,
        IKeyService keyService,
        IConfiguration config)
    {
        public async Task SendEmailLoginCodeAsync(string email, CancellationToken cancellationToken = default)
        {
            Task<string> appleReviewEmail = keyService.GetSecretAsync("Apple-Review-Email");
            Task<string> googleReviewEmail = keyService.GetSecretAsync("Google-Review-Email");

            if (email == await appleReviewEmail || email == await googleReviewEmail)
                return;

            Random random = new();
            string code = "";
            for (int i = 0; i < 6; i++)
            {
                code = code + random.Next(0, 10).ToString();
            }

            User user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new()
                {
                    UserName = email,
                    Email = email,
                    AccountStatus = UserAccountStatus.Prospective,
                };

                await userManager.CreateAsync(user);
            }

            if (user.ExternalId == default)
            {
                user.ExternalId = Guid.NewGuid();
            }

            if (user.OneSignalId == default)
            {
                user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email, cancellationToken);
            }

            await authRepository.CreateEmailLoginAsync(email, code, DateTimeOffset.UtcNow.AddMinutes(15), cancellationToken);

            await oneSignalService.SendTemplatedEmailAsync(config["ONESIGNAL_VERIFY_EMAIL_TEMPLATE_ID"], [email], new { code }, cancellationToken);
        }

        public async Task<(string Token, bool Onboarded)> VerifyEmailLoginAsync(string email, string code, CancellationToken cancellationToken = default)
        {
            Task<string> appleReviewEmail = keyService.GetSecretAsync("Apple-Review-Email");
            Task<string> googleReviewEmail = keyService.GetSecretAsync("Google-Review-Email");
            Task<string> appleReviewCode = keyService.GetSecretAsync("Apple-Review-Code");
            Task<string> googleReviewCode = keyService.GetSecretAsync("Google-Review-Code");

            bool isValid;
            if (email == await appleReviewEmail || email == await googleReviewEmail)
            {
                if (email == await appleReviewEmail && code == await appleReviewCode) isValid = true;
                else if (email == await googleReviewEmail && code == await googleReviewCode) isValid = true;
                else isValid = false;
            }
            else
            {
                isValid = await authRepository.IsEmailLoginCodeValidAsync(email, code, cancellationToken);
            }

            if (!isValid)
                throw new AuthenticationException();

            User user = await userManager.FindByEmailAsync(email);

            if (user.AccountStatus == UserAccountStatus.Prospective)
            {
                await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1", cancellationToken);
                await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1", cancellationToken);
            }

            user.EmailConfirmed = true;
            user.AccountStatus = UserAccountStatus.Active;

            if (user.FirstName == default)
            {
                user.FirstName = nameService.GetRandomFirstName();
            }
            if (user.LastName == default)
            {
                user.LastName = nameService.GetRandomLastName();
            }
            if (user.JoinDate == default)
            {
                user.JoinDate = DateTimeOffset.UtcNow;
                await oneSignalService.AddTagAsync(user.ExternalId, "joined_at", user.JoinDate.ToUnixTimeSeconds().ToString(), cancellationToken);
            }
            if (user.TimeOfUserAgreement == default)
            {
                user.TimeOfUserAgreement = DateTimeOffset.UtcNow;
            }
            if (user.StripeCustomerId == default)
            {
                var options = new CustomerCreateOptions
                {
                    Email = user.Email,
                };

                Customer customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
                user.StripeCustomerId = customer.Id;
            }
            if (user.CircleId == default)
            {
                await circleService.CreateCircleAsync(user.Id, $"{user.FirstName}'s Circle", cancellationToken: cancellationToken);
            }

            await userRepository.SaveUserAsync(user, cancellationToken);

            string token = await CreateLoginTokenAsync(user);

            return (token, user.FirstName != null && user.LastName != null);
        }

        public async Task<(string Token, bool Onboarded)> LoginWithAppleAsync(string email, string appleUserId, bool emailVerified, CancellationToken cancellationToken = default)
        {
            User user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new()
                {
                    UserName = email,
                    Email = email,
                };

                await userManager.CreateAsync(user);
            }

            if (user.ExternalId == default)
            {
                user.ExternalId = Guid.NewGuid();
            }
            if (user.OneSignalId == default)
            {
                user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email, cancellationToken);
            }
            if (user.AccountStatus == UserAccountStatus.Prospective)
            {
                await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1", cancellationToken);
                await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1", cancellationToken);
            }

            user.EmailConfirmed = emailVerified;
            user.AppleId = appleUserId;
            user.AccountStatus = UserAccountStatus.Active;

            if (user.FirstName == default)
            {
                user.FirstName = nameService.GetRandomFirstName();
            }
            if (user.LastName == default)
            {
                user.LastName = nameService.GetRandomLastName();
            }
            if (user.TimeOfUserAgreement == default)
            {
                user.TimeOfUserAgreement = DateTimeOffset.UtcNow;
            }
            if (user.JoinDate == default)
            {
                user.JoinDate = DateTimeOffset.UtcNow;
                await oneSignalService.AddTagAsync(user.ExternalId, "joined_at", user.JoinDate.ToUnixTimeSeconds().ToString(), cancellationToken);
            }
            if (user.StripeCustomerId == default)
            {
                var options = new CustomerCreateOptions
                {
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                };

                Customer customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
                user.StripeCustomerId = customer.Id;
            }
            if (user.CircleId == default)
            {
                await circleService.CreateCircleAsync(user.Id, $"My Circle", cancellationToken: cancellationToken);
            }

            await userRepository.SaveUserAsync(user, cancellationToken);

            string token = await CreateLoginTokenAsync(user);

            return (token, user.FirstName != null && user.LastName != null);
        }

        public async Task<(string Token, bool Onboarded)> LoginWithGoogleAsync(string email, string googleUserId, bool emailVerified, string firstName, string lastName, CancellationToken cancellationToken = default)
        {
            User user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new()
                {
                    UserName = email,
                    Email = email,
                };

                await userManager.CreateAsync(user);
            }

            if (user.ExternalId == default)
            {
                user.ExternalId = Guid.NewGuid();
            }
            if (user.OneSignalId == default)
            {
                user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email, cancellationToken);
            }
            if (user.AccountStatus == UserAccountStatus.Prospective)
            {
                await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1", cancellationToken);
                await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1", cancellationToken);
            }

            user.EmailConfirmed = emailVerified;
            user.GoogleId = googleUserId;
            user.FirstName = firstName ?? "";
            user.LastName = lastName ?? "";
            user.AccountStatus = UserAccountStatus.Active;

            if (user.TimeOfUserAgreement == default)
            {
                user.TimeOfUserAgreement = DateTimeOffset.UtcNow;
            }
            if (user.JoinDate == default)
            {
                user.JoinDate = DateTimeOffset.UtcNow;
                await oneSignalService.AddTagAsync(user.ExternalId, "joined_at", user.JoinDate.ToUnixTimeSeconds().ToString(), cancellationToken);
            }
            if (user.StripeCustomerId == default)
            {
                var options = new CustomerCreateOptions
                {
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                };

                Customer customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
                user.StripeCustomerId = customer.Id;
            }
            if (user.CircleId == default)
            {
                await circleService.CreateCircleAsync(user.Id, $"{user.FirstName}'s Circle", cancellationToken: cancellationToken);
            }

            await userRepository.SaveUserAsync(user, cancellationToken);

            string token = await CreateLoginTokenAsync(user);

            return (token, user.FirstName != null && user.LastName != null);
        }

        public async Task SetUserNameByEmailAsync(string email, string firstName, string lastName, CancellationToken cancellationToken = default)
        {
            User user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new()
                {
                    UserName = email,
                    Email = email,
                    AccountStatus = UserAccountStatus.Prospective,
                };

                await userManager.CreateAsync(user);
            }

            user.FirstName = firstName;
            user.LastName = lastName;

            await userRepository.SaveUserAsync(user, cancellationToken);
        }

        private async Task<string> CreateLoginTokenAsync(User user)
        {
            string signingKey = await keyService.GetSecretAsync("Cher-Ami-API-Signing-Key");

            return JwtBearer.CreateToken(o =>
            {
                o.SigningKey = signingKey;
                o.ExpireAt = DateTime.UtcNow.AddDays(10);
                o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                o.User.Claims.Add(new Claim("Email", user.Email));
            });
        }
    }
}

using FastEndpoints.Security;
using System;
using System.Security.Claims;

namespace CherAmiAPI.Services
{
    public static class AuthTokens
    {
        public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

        /// <summary>Past this point a request gets a fresh token, so anyone who opens the app inside a month never signs in again.</summary>
        public static readonly TimeSpan RenewWhenRemainingBelow = TimeSpan.FromDays(15);

        public const string RefreshedHeader = "X-Refreshed-Token";

        public static string Create(string signingKey, long userId, string email)
            => JwtBearer.CreateToken(o =>
            {
                o.SigningKey = signingKey;
                o.ExpireAt = DateTime.UtcNow.Add(Lifetime);
                o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                o.User.Claims.Add(new Claim("Email", email));
            });
    }
}

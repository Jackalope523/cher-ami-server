using CherAmiAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CherAmiAPI.Middleware
{
    /// <summary>
    /// Hands back a fresh token once the current one is over halfway through its life,
    /// so a signed-in user who keeps using the app is never signed out.
    /// </summary>
    public class SlidingTokenMiddleware(RequestDelegate next, IConfiguration config)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // Set before the response starts — after `next` the headers may already be sent.
            TryRenew(context);

            await next(context);
        }

        private void TryRenew(HttpContext context)
        {
            try
            {
                if (context.User?.Identity?.IsAuthenticated != true) return;

                string expires = context.User.FindFirstValue("exp");

                if (!long.TryParse(expires, out long expiresAt)) return;

                TimeSpan remaining = DateTimeOffset.FromUnixTimeSeconds(expiresAt) - DateTimeOffset.UtcNow;

                if (remaining > AuthTokens.RenewWhenRemainingBelow) return;

                string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                string email = context.User.FindFirstValue("Email");

                if (!long.TryParse(userId, out long id) || string.IsNullOrEmpty(email)) return;

                string signingKey = config["Cher-Ami-API-Signing-Key"];

                if (string.IsNullOrEmpty(signingKey)) return;

                context.Response.Headers[AuthTokens.RefreshedHeader] = AuthTokens.Create(signingKey, id, email);
            }
            catch (Exception ex)
            {
                // A failure here must never cost someone their request.
                Log.Error(ex, "Failed to renew token");
            }
        }
    }
}

using CherAmiAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Website
{
    internal static class WebsiteApiKey
    {
        private const string Scheme = "key ";

        public static async Task<bool> IsValidAsync(HttpContext context, IKeyService keyService, CancellationToken cancellationToken = default)
        {
            string authorization = context.Request.Headers.Authorization.ToString();

            if (!authorization.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase)) return false;

            string expected = await keyService.GetSecretAsync("Cher-Ami-API-Key");

            return !string.IsNullOrEmpty(expected) && authorization[Scheme.Length..] == expected;
        }
    }
}

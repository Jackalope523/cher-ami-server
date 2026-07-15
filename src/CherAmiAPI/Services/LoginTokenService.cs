using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using FastEndpoints.Security;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class LoginTokenService(IKeyService keyService) : ILoginTokenService
    {
        public async Task<string> CreateLoginTokenAsync(User user)
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

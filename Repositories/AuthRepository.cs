using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Repositories
{
    public class AuthRepository(ApplicationDbContext ctx) : IAuthRepository
    {
        public async Task CreateEmailLoginAsync(string email, string code, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
        {
            ctx.EmailLogins.Add(new EmailLogin { Email = email, Code = code, ExpiresAt = expiresAt });
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsEmailLoginCodeValidAsync(string email, string code, CancellationToken cancellationToken = default)
        {
            return await ctx.EmailLogins.AnyAsync(x => x.Email == email && x.Code == code && DateTimeOffset.UtcNow < x.ExpiresAt, cancellationToken: cancellationToken);
        }
    }
}

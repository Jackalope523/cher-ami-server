using CherAmiAPI.Contexts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    /// <summary>
    /// Marks the app's first-run flow as finished. Called once the user has
    /// their name and either started or joined a family circle.
    /// </summary>
    public class CompleteOnboardingEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/user/onboarding/complete");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await ctx.Users
                .Where(x => x.Id == userId)
                .ExecuteUpdateAsync(x => x.SetProperty(u => u.OnboardingCompleted, true), cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}

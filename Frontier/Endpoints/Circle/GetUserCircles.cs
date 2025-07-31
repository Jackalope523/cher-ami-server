using FastEndpoints;
using Frontier.Contracts.Requests;
using Microsoft.AspNetCore.Identity;
using Repository.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Circle
{
    public class GetUserCircles(IAccountOperations accounts) : Endpoint<UserIdRequest, List<CircleShard>>
    {
        public override void Configure()
        {
            Get("/circle");
        }

        public override async Task HandleAsync(UserIdRequest request, CancellationToken cancellationToken)
        {
            CoreUser user = await userManager.GetUserAsync(HttpContext.User);

            return Ok(await circles.GetUserCirclesAsync(user.Id));
        }
    }
}
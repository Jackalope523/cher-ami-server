using FastEndpoints;
using Frontier.Contracts.Requests;
using Microsoft.AspNetCore.Identity;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Circle
{
    public class GetUserCircles(ICircleOperations circles) : EndpointWithoutRequest<List<CircleShard>>
    {
        public override void Configure()
        {
            Get("/circle");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            //long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //await Send.OkAsync(await circles.GetUserCirclesAsync(userId));

            throw new NotImplementedException();
        }
    }
}
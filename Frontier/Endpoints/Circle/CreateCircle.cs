using FastEndpoints;
using Frontier.Contracts.Requests;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class CreateCircle(ICircleService circles) : Endpoint<CircleCreationManifest>
    {
        public override void Configure()
        {
            Post("/circle");
            AllowFileUploads();
        }

        public override async Task HandleAsync(CircleCreationManifest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using MemoryStream stream = new();
            await request.Image.CopyToAsync(stream);

            CoreCircle coreCircle = await circles.CreateCircleAsync(
                                        userId,
                                        request.Title,
                                        request.Plan,
                                        request.Schedule,
                                        stream
                                    );

            CircleShard response = new CircleShard(
                coreCircle.Id,
                coreCircle.InviteCode,
                coreCircle.Title,
                coreCircle.DateCreated,
                coreCircle.Plan,
                coreCircle.Schedule
            );


            await Send.CreatedAtAsync<GetCircle>(response, cancellation: cancellationToken);
        }
    }
}
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class GetHeaderEndpoint(CircleService circleService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/circle/{id}/header");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            MemoryStream image = await circleService.GetHeaderAsync(userId, request.Id, cancellationToken);

            await Send.StreamAsync(image, cancellation: cancellationToken);
        }
    }
}

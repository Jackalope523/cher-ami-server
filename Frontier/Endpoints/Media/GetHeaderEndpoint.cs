using Core.Boundaries;
using FastEndpoints;
using LazyLizardBackend.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Media
{
    public class GetHeaderEndpoint(IMediaService mediaService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/media/headers/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            MemoryStream imageStream = await mediaService.GetHeaderAsync(userId, request.Id);

            if (imageStream != null)
            {
                imageStream.Seek(0, SeekOrigin.Begin);

                FileStreamResult response = new(imageStream, "image/jpeg")
                {
                    FileDownloadName = "header.jpg"
                };

                await Send.OkAsync(response, cancellationToken);
            }
            else
            {
                await Send.NotFoundAsync(cancellationToken);
            }
        }
    }
}
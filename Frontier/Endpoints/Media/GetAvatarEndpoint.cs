using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;

namespace CrazyLizard.Endpoints.Media
{
    public class GetAvatarEndpoint(IMediaService mediaService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/media/avatars/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            MemoryStream imageStream = await mediaService.GetAvatarAsync(userId, request.Id);

            if (imageStream != null)
            {
                imageStream.Seek(0, SeekOrigin.Begin);

                FileStreamResult response = new(imageStream, "image/jpeg")
                {
                    FileDownloadName = "avatar.jpg"
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
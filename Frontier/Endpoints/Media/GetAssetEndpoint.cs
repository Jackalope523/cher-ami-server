using CrazyLizard.Interfaces.Service;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Media
{
    public class AssetRequest
    {
        public string AssetFilename { get; set; }
    }

    public class GetAssetEndpoint(IMediaService mediaService) : Endpoint<AssetRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/media/assets/{id}");
        }

        public override async Task HandleAsync(AssetRequest request, CancellationToken cancellationToken)
        {
            MemoryStream imageStream = await mediaService.GetAssetAsync(request.AssetFilename);

            if (imageStream != null)
            {
                imageStream.Seek(0, SeekOrigin.Begin);

                FileStreamResult response = new(imageStream, "image/jpeg")
                {
                    FileDownloadName = $"{request.AssetFilename}.png"
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
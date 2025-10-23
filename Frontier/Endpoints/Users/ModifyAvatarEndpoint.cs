using CrazyLizard.Interfaces.Service;
using CrazyLizard.Shared.Requests;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Users
{
    public class UpdateAvatarEndpoint(IAccountService accountService) : Endpoint<ImageRequest>
    {
        public override void Configure()
        {
            Post("/account/avatar");
            AllowFileUploads();
        }

        public override async Task HandleAsync(ImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using var stream = new MemoryStream();
            await request.Image.CopyToAsync(stream);

            await accountService.EditAvatarAsync(userId, stream);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Media
{
    public class GetRecipientAvatarEndpoint(RecipientService recipientService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/recipients/{id}/avatar");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            MemoryStream image = await recipientService.GetAvatarAsync(userId, request.Id, cancellationToken);

            await Send.StreamAsync(image, cancellation: cancellationToken);
        }
    }
}

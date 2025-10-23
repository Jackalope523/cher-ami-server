using CrazyLizard.Entities;
using CrazyLizard.Shared.Responses;
using FastEndpoints;

namespace CrazyLizard.Shared.SharedMappers
{
    public class RecipientItemMapper : ResponseMapper<RecipientItem, Recipient>
    {
        public override RecipientItem FromEntity(Recipient recipient) => new()
        {
            Id = recipient.Id,
            ManagerId = recipient.ManagerId,
            FirstName = recipient.FirstName,
            LastName = recipient.LastName,
            AvatarPath = $"/recipients/{recipient.Id}/avatar",
            AvatarTimestamp = recipient.AvatarTimestamp,
        };
    }
}

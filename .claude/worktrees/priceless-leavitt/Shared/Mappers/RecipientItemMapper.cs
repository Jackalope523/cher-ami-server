using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.Extensions.Configuration;

namespace CherAmiAPI.Shared.Mappers
{
    public class RecipientItemMapper(IConfiguration config) : ResponseMapper<RecipientItem, Recipient>
    {
        public override RecipientItem FromEntity(Recipient recipient) => new()
        {
            Id = recipient.Id,
            ManagerId = recipient.ManagerId,
            Name = recipient.Name,
            AvatarUrl = recipient.AvatarPath == null ? null : $"{config["APP_SERVICE_URI"]}/recipients/{recipient.Id}/avatar?timestamp={recipient.AvatarTimestamp}",
            AvatarPath = recipient.AvatarPath == null ? null : $"/recipients/{recipient.Id}/avatar",
            AvatarTimestamp = recipient.AvatarTimestamp,
            IsVeteran = recipient.IsVeteran,
        };
    }
}

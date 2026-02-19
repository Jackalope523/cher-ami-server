using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;

namespace CherAmiAPI.Shared.Mappers
{
    public class RecipientMapper : Mapper<RecipientRequest, RecipientDTO, Recipient>
    {
        public override RecipientDTO FromEntity(Recipient recipient) => new()
        {
            Id = recipient.Id,
            ManagerId = recipient.ManagerId,
            AvatarPath = recipient.AvatarPath == null ? null : $"/recipients/{recipient.Id}/avatar",
            AvatarTimestamp = recipient.AvatarTimestamp,
            Title = recipient.Title,
            Name = recipient.Name,
            AddressLine1 = recipient.AddressLine1,
            AddressLine2 = recipient.AddressLine2,
            City = recipient.City,
            ProvinceOrState = recipient.ProvinceOrState,
            PostalCode = recipient.PostalCode,
            Country = recipient.Country,
        };

        public override Recipient ToEntity(RecipientRequest recipient) => new()
        {
            Title = recipient.Title,
            Name = recipient.Name,
            AddressLine1 = recipient.AddressLine1,
            AddressLine2 = recipient.AddressLine2,
            City = recipient.City,
            ProvinceOrState = recipient.ProvinceOrState,
            PostalCode = recipient.PostalCode,
            Country = recipient.Country,
        };
    }
}

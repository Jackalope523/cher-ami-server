using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using CherAmiAPI.Migrations.AzureSQLProduction;

namespace CherAmiAPI.Shared.Mappers
{
    public class RecipientMapper(IConfiguration config) : Mapper<RecipientRequest, RecipientDTO, Recipient>
    {
        public override RecipientDTO FromEntity(Recipient recipient) => new()
        {
            Id = recipient.Id,
            ManagerId = recipient.ManagerId,
            ManagerName = recipient.Manager == null ? null : $"{recipient.Manager.FirstName} {recipient.Manager.LastName}".Trim(),
            AvatarUrl = recipient.AvatarPath == null ? null : $"{config["APP_SERVICE_URI"]}/recipients/{recipient.Id}/avatar?timestamp={recipient.AvatarTimestamp}",
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
            IsVeteran = recipient.IsVeteran,
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
            IsVeteran = recipient.IsVeteran ?? false,
        };
    }
}

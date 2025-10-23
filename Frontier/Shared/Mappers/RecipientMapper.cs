using CrazyLizard.Entities;
using CrazyLizard.Shared.Requests;
using CrazyLizard.Shared.Responses;
using FastEndpoints;

namespace CrazyLizard.Shared.Mappers
{
    public class RecipientMapper : Mapper<RecipientRequest, RecipientDTO, Recipient>
    {
        public override RecipientDTO FromEntity(Recipient recipient) => new()
        {
            Id = recipient.Id,
            ManagerId = recipient.ManagerId,
            AvatarPath = $"/recipients/{recipient.Id}/avatar",
            Title = recipient.Title,
            FirstName = recipient.FirstName,
            LastName = recipient.LastName,
            Street = recipient.Street,
            City = recipient.City,
            ProvinceOrState = recipient.ProvinceOrState,
            PostalCode = recipient.PostalCode,
            Country = recipient.Country,
            UnitNumber = recipient.UnitNumber,
        };

        public override Recipient ToEntity(RecipientRequest recipient) => new()
        {
            Title = recipient.Title,
            FirstName = recipient.FirstName,
            LastName = recipient.LastName,
            Street = recipient.Street, 
            City = recipient.City,
            ProvinceOrState = recipient.ProvinceOrState,
            PostalCode = recipient.PostalCode,
            Country = recipient.Country,
            UnitNumber = recipient.UnitNumber,
   
        };
    }
}

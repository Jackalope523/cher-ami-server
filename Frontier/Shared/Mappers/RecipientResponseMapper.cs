using CrazyLizard.Entities;
using CrazyLizard.Shared.Responses;
using FastEndpoints;

namespace CrazyLizard.Shared.SharedMappers
{
    public class RecipientResponseMapper : ResponseMapper<RecipientDTO, Recipient>
    {
        public override RecipientDTO FromEntity(Recipient recipient) => new RecipientDTO()
        {
            Id = recipient.Id,
            ManagerId = recipient.ManagerId,
            Title = recipient.Title,
            FirstName = recipient.FirstName,
            LastName = recipient.LastName,
            Street = recipient.Street,
            ProvinceOrState = recipient.ProvinceOrState,
            City = recipient.City, 
            Country = recipient.Country, 
            PostalCode = recipient.PostalCode,
            UnitNumber = recipient.UnitNumber
        };
    }
}
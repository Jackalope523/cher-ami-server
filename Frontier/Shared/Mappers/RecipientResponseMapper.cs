using Core.Boundaries;
using CrazyLizard.Shared.Responses;
using FastEndpoints;

namespace CrazyLizard.Shared.SharedMappers
{
    public class RecipientResponseMapper : ResponseMapper<RecipientDTO, CoreRecipient>
    {
        public override RecipientDTO FromEntity(CoreRecipient coreRecipient) => new RecipientDTO()
        {
            Id = coreRecipient.Id,
            ManagerId = coreRecipient.ManagerId,
            FullName = $"{coreRecipient.Title} {coreRecipient.FirstName} {coreRecipient.LastName}",
            Address = coreRecipient.Address,
        };
    }
}
using Core.Boundaries;
using FastEndpoints;
using Frontier.Contracts.Responses;

namespace CrazyLizard.Shared.SharedMappers
{
    public class RecipientResponseMapper : ResponseMapper<RecipientDTO, CoreRecipient>
    {
        public override RecipientDTO FromEntity(CoreRecipient coreRecipient) => new RecipientDTO()
        {
            Id = coreRecipient.Id,
            ManagerId = coreRecipient.ManagerId,
            FullName = $"{coreRecipient.Title} {coreRecipient.FirstName} {coreRecipient.LastName}",
            DateOfBirth = coreRecipient.DateOfBirth,
            Address = coreRecipient.Address,
        };
    }
}
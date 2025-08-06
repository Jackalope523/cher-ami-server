using FastEndpoints;
using Frontier.Contracts.Responses;

namespace Mappers
{
    public class RecipientResponseMapper : ResponseMapper<RecipientShard, CoreRecipient>
    {
        public override RecipientShard FromEntity(CoreRecipient coreRecipient) => new RecipientShard()
        {
            Id = coreRecipient.Id,
            ManagerId = coreRecipient.ManagerId,
            FullName = $"{coreRecipient.Title} {coreRecipient.FirstName} {coreRecipient.LastName}",
            DateOfBirth = coreRecipient.DateOfBirth,
            Address = coreRecipient.Address,
        };
    }
}
using FastEndpoints;
using System.Security.Claims;

namespace Frontier.Contracts.Requests
{
    public class UserIdRequest
    {
        [FromClaim(ClaimTypes.NameIdentifier)]
        public long UserId { get; set; }
    }
}

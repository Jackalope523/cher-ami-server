using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Shared.Responses;

namespace CrazyLizard.Shared.Mappers
{
    public class PostResponseMapper : ResponseMapper<PostDTO, CorePost>
    {
        public override PostDTO FromEntity(CorePost post) => new()
        {
            Id = post.Id,
            IssueId = post.IssueId,
            UserId = post.UserId,
            Timestamp = post.Timestamp,
            Caption = post.Caption,
        };
    }
}

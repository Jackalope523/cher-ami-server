using FastEndpoints;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Entities;

namespace CrazyLizard.Shared.Mappers
{
    public class PostResponseMapper : ResponseMapper<PostDTO, Post>
    {
        public override PostDTO FromEntity(Post post) => new()
        {
            Id = post.Id,
            IssueId = post.IssueId,
            AuthorId = post.AuthorId,
            PostedAt = post.PostedAt,
            Caption = post.Caption,
        };
    }
}

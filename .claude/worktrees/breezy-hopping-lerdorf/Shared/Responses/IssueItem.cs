
namespace CherAmiAPI.Shared.Responses
{
    public record IssueItem
    {
        public long Id { get; init; }
        public string Title { get; init; }
        public int PostCount { get; init; }
    }
}

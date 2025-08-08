namespace Frontier.Contracts.Responses
{
    public record UserDTO
    {
        public long Id { get; init; }
        public string FirstName { get; init; }
        public string FamilyName { get; init; }
    }
}

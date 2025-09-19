namespace CrazyLizard.Shared.Responses
{
    public record UserDTO
    {
        public long Id { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string AvatarPath { get; init; }
    }
}

namespace LazyLizardBackend.Shared.Responses
{
    public record ImageMetadataDTO
    {
        public string Hash { get; init; }
        public bool Concealed { get; init; }
    }
}

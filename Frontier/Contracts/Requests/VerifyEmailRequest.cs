namespace LazyLizardBackend.Contracts.Requests
{
    public class VerifyEmailRequest
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}

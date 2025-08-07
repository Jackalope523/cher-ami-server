using System.ComponentModel.DataAnnotations;

namespace LazyLizardBackend.Contracts.Requests
{
    public class VerifyLoginRequest
    {
        [Required]
        public string PhoneNumber { get; set; }

        public string Code { get; set; }
    }
}

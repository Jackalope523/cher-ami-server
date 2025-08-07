using System.ComponentModel.DataAnnotations;

namespace LazyLizardBackend.Contracts.Requests
{
    public class LoginRequest
    {
        [Required]
        public string PhoneNumber { get; set; }

        public bool? UseWhatsApp { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System;

namespace LazyLizardBackend.Contracts.Requests
{
    public class CreateAccountRequest
    {
        [Required]
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        [Required]
        public string GivenName { get; set; }
        [Required]
        public string FamilyName { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string CircleCode { get; set; }
    }
}

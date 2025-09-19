using Core.Boundaries;
using CrazyLizard.Entities;
using System;

namespace CrazyLizard.Shared.Responses
{
    public record AccountDTO
    {
        public long Id { get; init; }
        public string PhoneNumber { get; init; }
        public string Email { get; init; }
        public string Title { get; init; }
        public string GivenName { get; init; }
        public string FamilyName { get; init; }
        public DateOnly DateOfBirth { get; init; }
        public bool IsPhoneConfirmed { get; init; }
        public bool IsEmailConfirmed { get; init; }
        public UserAccountStatus AccountStatus { get; init; }
        public DateTimeOffset JoinDate { get; init; }
        public DateTimeOffset TimeOfUserAgreement { get; init; }
        public Guid NotificationId { get; init; }
    }
}

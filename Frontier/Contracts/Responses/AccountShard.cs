using System;

namespace Frontier.Contracts.Responses
{
    public record AccountShard
    {
        public long Id { get; init; }
        public string PhoneNumber { get; init; }
        public string Email { get; init; }
        public string Title { get; init; }
        public string GivenName { get; init; }
        public string FamilyName { get; init; }
        public DateTimeOffset DateOfBirth { get; init; }
        public bool IsPhoneConfirmed { get; init; }
        public bool IsEmailConfirmed { get; init; }
        public UserAccountStatus AccountStatus { get; init; }
        public DateTimeOffset JoinDate { get; init; }
        public DateTimeOffset TimeOfUserAgreement { get; init; }
        public Guid NotificationId { get; init; }
    }
}

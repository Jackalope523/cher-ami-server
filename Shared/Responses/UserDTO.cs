using CherAmiAPI.Entities;
using System;
using System.Collections.Generic;

namespace CherAmiAPI.Shared.Responses
{
    public record UserDTO
    {
        public long Id { get; init; }
        public string ExternalId { get; init; }
        public string AvatarUrl { get; init; }
        public string AvatarPath { get; init; }
        public DateTimeOffset? AvatarTimestamp { get; init; }
        public string Title { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public DateTimeOffset JoinDate { get; init; }
        public bool IsBillingExempt { get; init; }
        public bool NameProvidedByUser { get; init; }
        public bool OnboardingCompleted { get; init; }

        public bool PushNewPosts { get; init; }
        public bool PushIssueReminders { get; init; }
        public bool PushNewMembers { get; init; }
        public bool EmailIssueReminders { get; init; }
        public bool EmailMarketing { get; init; }
        
        public List<RecipientItem> Recipients { get; init; }
    }
}

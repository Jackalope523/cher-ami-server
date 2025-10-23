using CrazyLizard.Entities;
using System;
using System.Collections.Generic;

namespace CrazyLizard.Shared.Responses
{
    public record UserDTO
    {
        public long Id { get; init; }
        public string AvatarPath { get; init; }
        public DateTimeOffset AvatarTimestamp { get; init; }
        public string Title { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public DateTimeOffset? JoinDate { get; init; }
        public List<RecipientItem> Recipients { get; init; }
    }
}

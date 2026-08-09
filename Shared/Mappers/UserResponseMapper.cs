using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace CherAmiAPI.Shared.Mappers
{
    public class UserResponseMapper(IConfiguration config, RecipientItemMapper mapper) : ResponseMapper<UserDTO, User>
    {
        public override UserDTO FromEntity(User user) => new()
        { 
            Id = user.Id,
            ExternalId = user.ExternalId.ToString(),
            AvatarUrl = user.AvatarPath == null ? null : $"{config["APP_SERVICE_URI"]}/users/{user.Id}/avatar?timestamp={user.AvatarTimestamp}",
            AvatarPath = user.AvatarPath == null ? null : $"/users/{user.Id}/avatar",
            AvatarTimestamp = user.AvatarTimestamp,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            JoinDate = user.JoinDate,
            IsBillingExempt = user.IsBillingExempt,
            NameProvidedByUser = user.NameProvidedByUser,
            OnboardingCompleted = user.OnboardingCompleted,
            Recipients = [.. user.Recipients.Select(mapper.FromEntity)],
        };
    }
}

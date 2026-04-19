using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace CherAmiAPI.Shared.SharedMappers
{
    public class CircleResponseMapper(IConfiguration config, UserItemMapper userItemMapper, RecipientItemMapper recipientItemMapper) : ResponseMapper<CircleDTO, Circle>
    {
        public override CircleDTO FromEntity(Circle circle) 
        {
            return new()
            {
                Id = circle.Id,
                HeaderUrl = $"{config["APP_SERVICE_URI"]}/circle/{circle.Id}/header?timestamp=${circle.HeaderTimestamp}",
                HeaderPath = $"/circle/{circle.Id}/header",
                HeaderTimestamp = circle.HeaderTimestamp,
                Title = circle.Title,
                InviteCode = circle.CircleCode,
                DateCreated = circle.TimeOfCreation,
                Schedule = circle.IssueSchedule,
                Contributors = circle.Contributors.Select(userItemMapper.FromEntity).ToList(),
                Recipients = circle.Contributors.SelectMany(x => x.Recipients).Select(recipientItemMapper.FromEntity).ToList(),
            };
        } 
    }
}

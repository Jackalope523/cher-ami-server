using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using System.Linq;

namespace CherAmiAPI.Shared.SharedMappers
{
    public class CircleResponseMapper(UserItemMapper userItemMapper, RecipientItemMapper recipientItemMapper) : ResponseMapper<CircleDTO, Circle>
    {
        public override CircleDTO FromEntity(Circle circle) 
        {
            return new()
            {
                Id = circle.Id,
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

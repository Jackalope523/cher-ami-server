using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class CircleService(ApplicationDbContext ctx, IImageService imageService, IInviteCodeService inviteCodeService)
    {
        public async Task<Circle> CreateCircleAsync(long userId, string title, IFormFile headerImage = null, CancellationToken cancellationToken = default)
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                string code = await inviteCodeService.GenerateCodeAsync();

                Circle toCreate = new()
                {
                    Title = title,
                    TimeOfCreation = DateTimeOffset.UtcNow,
                    CircleCode = code,
                    IssueSchedule = IssueSchedule.Monthly,
                };

                ctx.Circles.Add(toCreate);
                await ctx.SaveChangesAsync(cancellationToken);

                if (headerImage != null)
                {
                    string path = $"circles/{toCreate.Id}/header/header.jpg";

                    using var stream = new MemoryStream();
                    await headerImage.CopyToAsync(stream, cancellationToken);

                    toCreate.HeaderPath = path;
                    toCreate.HeaderTimestamp = DateTimeOffset.UtcNow;

                    await imageService.UploadImageAsync(path, stream);
                }
               
                DateTime now = DateTime.UtcNow;
                DateTime endOfMonth = new(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                TimeSpan untilEnd = endOfMonth - now;
                bool lessThan7DaysUntilMonthEnd = untilEnd.TotalDays < 7;

                DateTime draftingEnd = lessThan7DaysUntilMonthEnd
                    ? new DateTime(now.Year, now.Month, 1).AddMonths(2).AddTicks(-1)
                    : new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1);

                Issue firstIssue = new()
                {
                    CircleId = toCreate.Id,
                    Title = $"{draftingEnd:MMMM yyyy} · Issue 1",
                    IssueNumber = 1,
                    DraftingStart = DateTimeOffset.UtcNow,
                    DraftingEnd = new DateTimeOffset(draftingEnd, TimeSpan.Zero),
                    Status = IssueStatus.Drafting,
                    HeaderPath = null,
                };

                ctx.Issues.Add(firstIssue);

                User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);
                user.CircleId = toCreate.Id;
                user.CircleJoinDate = DateTimeOffset.UtcNow;

                await ctx.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return toCreate;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}

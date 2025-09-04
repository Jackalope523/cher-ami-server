using Core.Boundaries;
using CrazyLizard.Contexts;
using System;
using System.Threading.Tasks;

namespace CrazyLizard.Repositories
{
    public class MiscellaneousRepository(CrazyLizardContext ctx) : IMiscellaneousRepository
    {
        public async Task SaveFeedbackAsync(string comments, DateTimeOffset time)
        {
            ctx.Feedback.Add(new() { Comments = comments, Time = time });
            await ctx.SaveChangesAsync();
        }

        public async Task SaveFeedbackAsync(string comments, DateTimeOffset time, long userId)
        {
            ctx.Feedback.Add(new() { Comments = comments, Time = time, UserId = userId });
            await ctx.SaveChangesAsync();
        }
    }
}

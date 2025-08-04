using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static Core.Entities.Arbiter;
using static Core.Entities.Artificer;
using static Core.Entities.Psijic;

namespace Core.Controls
{
    public class CircleDirector : AbstractDirector, ICircleOperations
	{
		public CircleDirector(CoreTerminal terminal) : base(terminal) { }

        public Task<List<Entities.CoreCircle>> GetUserCirclesAsync(long userId)
        {
            throw new NotImplementedException();
        }

        public Task<Entities.CoreCircle> GetCircleInformationAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<Entities.CoreCircle> CreateCircleAsync(long userId, string title, CirclePlan plan, IssueSchedule schedule, MemoryStream header)
        {
            throw new NotImplementedException();
        }

        public Task EditCircleAsync(long userId, long circleId, string title = "", CirclePlan? plan = null, IssueSchedule? schedule = null, MemoryStream header = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> RerollCircleCodeAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCircleAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CircleMembershipShard>> GetMembersForCircleAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task SendInvitationAsync(long userId, long circleId, string phoneNumber = null, string email = null)
        {
            throw new NotImplementedException();
        }

        public Task JoinCircleAsync(long userId, string circleCode)
        {
            throw new NotImplementedException();
        }

        public Task RemoveMemberAsync(long userId, long circleId, long targetId)
        {
            throw new NotImplementedException();
        }

        public Task<List<RecipientShard>> GetRecipientsForCircleAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task AddRecipientAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task EditRecipientAsync(long recipientId, List<(string Property, object Value)> edits)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRecipientAsync(long recipientId)
        {
            throw new NotImplementedException();
        }
    }
}

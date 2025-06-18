using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Notifications;

using static Core.Entities.Arbiter;
using static Core.Entities.Psijic;

namespace Core.Entities
{
    using static CoreTerminal;

    internal class Circle
    {
        #region Variables

        //////
        // Constants
        //////////////

        public const int MaximumTitleLength = 30;

        public static Circle None
            => new() { Id = -1, Exists = false };

        ///////
        // Properties
        ///////////////

        public long Id { get; init; }
        public string Title { get; set; }
        public string InviteCode { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public CirclePlan Plan { get; set; }
        public IssueSchedule Schedule { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsActive
            => !Plan.Equals(CirclePlan.None);

        public bool Exists { get; set; } = true;

        ////////
        // Synced Properties
        //////////////////////
        
        public Synced<List<CircleMember>> Members { get; }
        public Synced<List<CoreRecipient>> Recipients { get; }

        public Synced<List<Issue>> Issues { get; }

        #endregion

        #region Initialisation & Extraction

        public static async Task<Circle> GetCircleAsync(long id)
        {
            return new(await Terminal.CircleDatabase.GetCircleAsync(id));
        }

        public Circle()
        {
            Members = new(() => Terminal.CircleDirector.RequestAllUsersFromGatheringAsync(this));

            Issues = new(() => Terminal.IssueDirector.RequestGatheringSnapshotsAsync(this));
        }

        public Circle(CoreCircle fromCircle) : this()
        {
            Id = fromCircle.Id;
            InviteCode = fromCircle.InviteCode;
            Title = fromCircle.Title;
            DateCreated = fromCircle.DateCreated;
            Plan = fromCircle.Plan;
            Schedule = fromCircle.Schedule;
            IsDeleted = fromCircle.IsPendingDeletion;
        }

        public CoreCircle ToCoreCircle()
        {
            return new(Id, InviteCode, Title,
                DateCreated, Plan, Schedule,
                IsDeleted);
        }

        public CircleShard ToCircleShard()
        {
            return new(Id, InviteCode, Title,
                DateCreated, Plan, Schedule);
        }

		#endregion

		#region Composition

		public bool ValidateAndNormalise(out string issues)
        {
            issues = "";

            // Sanitise User content
            Title = ContentValidation.NormaliseText(Title, MaximumTitleLength);
            if (string.IsNullOrEmpty(Title)) { issues += "Title cannot be empty. "; }

            return issues.Equals("");
        }

		#endregion

		#region Checks

        public async Task<bool> HasMember(User user)
        {
            return (await Members).Contains(user);
        }

        public async Task<bool> IsModifiableBy(User user)
        {
            var admins = (await Members).Where(member => member.MembershipType.Equals(CircleMembershipType.Owner));

            if (admins.Contains(user))
			{ return true; }

			return false;
        }

		#endregion

		#region Effects

        #endregion

        #region Actions

        public async Task<string> NotifyMembers(CardinalNotification notification, DateTimeOffset? notifyAt = null)
        {
            return await Terminal.NotificationDirector.NotifyUsersAsync(notification, notifyAt, (await Members).ToArray());
        }

		#endregion

		#region Dissimilation

		public override bool Equals(object obj)
		{
			return obj is Circle other &&
                Exists == other.Exists &&
                Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		#endregion
	}
}

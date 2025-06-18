using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;

using static Core.Entities.Arbiter;
using static Core.Entities.Psijic;

namespace Core.Entities
{
    using static CoreTerminal;

    internal class Issue
    {
		#region Variables

		//////
		// Constants
		//////////////

		public const int MaximumTitleLength = 30;

        public static Issue None
            => new() { Id = -1, Exists = false };

		///////
		// Properties
		///////////////

        // Core
		public long Id { get; init; }
        public long CircleId { get; init; }
        public string Title { get; set; }
        public IssueType Type { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public bool IsUpcoming
            => HasYet(StartDate);
        public bool IsOpen
            => HasAlready(StartDate) && HasYet(EndDate);

        public bool Exists { get; set; } = true;

        ////////
        // Synced Properties
        //////////////////////
        
        public Synced<Circle> Circle { get; }

        public Synced<List<PostShard>> Posts { get; }

        public Synced<List<CoreOrder>> Orders { get; }

        #endregion

        #region Initialisation & Extraction

        public static async Task<Issue> GetIssueAsync(long id)
        {
            return new(await Terminal.IssueDatabase.GetIssueAsync(id));
        }

        public Issue()
        {
            Circle = new(() => Entities.Circle.GetCircleAsync(CircleId));
            Posts = new(() => Terminal.IssueDirector.RequestGatheringSnapshotsAsync(this));
            Orders = new(() => Terminal.IssueDirector.RequestGatheringSnapshotsAsync(this));
        }

        public Issue(CoreIssue fromIssue) : this()
        {
            Id = fromIssue.Id;
            CircleId = fromIssue.CircleId;
            Title = fromIssue.Title;
            Type = fromIssue.Type;
            StartDate = fromIssue.StartDate;
            EndDate = fromIssue.EndDate;
        }

        public CoreIssue ToCoreIssue()
        {
            return new(Id, CircleId, Type,
                Title, StartDate, EndDate);
        }

        public IssueShard ToIssueShard()
        {
            return new(Id, CircleId, Type,
                Title, StartDate, EndDate);
        }

        public ProfileIssueShard ToProfileIssueShard()
        {
            return new(Id, EndDate);
        }

		#endregion

		#region Composition

		public bool ValidateAndNormalise(out string issues)
        {
            issues = "";

            // Sanitise User content
            Title = ContentValidation.NormaliseText(Title, MaximumTitleLength);
            if (string.IsNullOrEmpty(Title)) { issues += "Title cannot be empty. "; }

            // If in the past, make it now
            if (HappenedBefore(StartDate, Time)) { StartDate = Time; }

            // Verify end date after start date
            if (HappenedBefore(EndDate, StartDate)) { issues += "End date before start date. "; }

            return issues.Equals("");
        }

        public async Task<List<PostShard>> GetPostsOf(User user)
        {
            return (await Posts).Where(snapshot => snapshot.UserId.Equals(user.Id)).ToList();
        }

		#endregion

		#region Checks

		public async Task<bool> IsVisibleTo(User user)
		{
            return await (await Circle).HasMember(user);
		}

		#endregion

		#region Effects

        public async Task<List<User>> Ended()
        {
            List<User> updatedGuests = new();

            // Update all participants' vectors and notify
			foreach ((var guest, var joined, var left) in await GuestHistory)
			{
                if (left.HasValue)
                { guest.CalculateCharacter(this, left.Value - joined); }
                else
                { guest.CalculateCharacter(this, Time - joined); }

                updatedGuests.Add(guest);
			}

            return updatedGuests;
		}

        public async Task Taken(User user)
        {
            // Verify snapshot is not before gathering starting or user is host
            Verify(HasAlready(StartDate) || IsModifiableBy(user),
                new UserErrorException(GatheringErrorCode.NOT_STARTED));

            // Verify user can etch into the gathering
            Verify(await WasAttendedBy(user) || IsModifiableBy(user),
                new UserErrorException(GatheringErrorCode.NOT_GUEST));
		}

        #endregion

        #region Actions

        public async Task Export()
        {

        }

		#endregion

		#region Dissimilation

		public override bool Equals(object obj)
		{
			return obj is Issue other &&
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

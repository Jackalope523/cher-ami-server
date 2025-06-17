using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;

using static Core.Entities.Arbiter;
using static Core.Entities.Psijic;
using Microsoft.Extensions.Logging;
using Core.Notifications;

namespace Core.Entities
{
    using static CoreTerminal;

    internal class Group
    {
        #region Variables

        //////
        // Constants
        //////////////

        public const int MaximumTitleLength = 30;

        public static Group None
            => new() { Id = 0, Exists = false };

        ///////
        // Properties
        ///////////////

        public long Id { get; init; }
        public long OwnerId { get; init; }
        public string Title { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public GroupPlan Plan { get; set; }
        public SegmentFrequency Frequency { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsActive
            => !Plan.Equals(GroupPlan.None);

        public bool Exists { get; set; } = true;

        ////////
        // Synced Properties
        //////////////////////
        
        public Synced<User> Owner { get; }
        public Synced<List<User>> Members { get; }

        public Synced<List<Segment>> Segments { get; }

        #endregion

        #region Initialisation & Extraction

        public static async Task<Group> GetGroupAsync(long id)
        {
            return new(await Terminal.GatheringDatabase.FindGatheringAsync(id));
        }

        public Group()
        {
            Owner = new(() => User.GetUserAsync(OwnerId));
            Members = new(() => Terminal.GatheringDirector.RequestAllUsersFromGatheringAsync(this));

            Segments = new(() => Terminal.SnapshotDirector.RequestGatheringSnapshotsAsync(this));
        }

        public Group(CoreGroup fromGroup) : this()
        {
            Id = fromGroup.Id;
            OwnerId = fromGroup.OwnerId;
            Title = fromGroup.Title;
            DateCreated = fromGroup.DateCreated;
            Plan = fromGroup.Plan;
            Frequency = fromGroup.Frequency;
            IsDeleted = fromGroup.IsPendingDeletion;
        }

        public CoreGroup ToCoreGroup()
        {
            return new(Id, OwnerId, Title, Description,
                StartTime, Location.Latitude, Location.Longitude, FriendlyLocation,
                EndTime, State, GroupMinimum, GroupMaximum, Character.ToCharacter(),
                Radius.Kilometres, IsDynamic, IsDeleted, NumberOfGuests,
                DegreeOfPrivacy, Visibility, TimeOfCreation, Decay);
        }

        public async Task<GroupShard> ToGroupShard()
        {
            return new(Id, (await Owner).ToUserShard(), Title, Description,
                StartTime, Location.Latitude, Location.Longitude, FriendlyLocation,
                EndTime, State, GroupMinimum, GroupMaximum,
                Radius.Kilometres, DegreeOfPrivacy, NumberOfGuests, RelativeAngle,
                Visibility, Decay);
        }

		#endregion

		#region Composition

		public bool ValidateAndNormalise(out string issues)
        {
            issues = "";

            // Sanitise User content
            Title = ContentValidation.NormaliseText(Title, MaximumTitleLength);
            if (string.IsNullOrEmpty(Title)) { issues += "Title cannot be empty. "; }

            if (!string.IsNullOrEmpty(Description))
            { Description = ContentValidation.NormaliseText(Description, MaximumDescLength); }

            FriendlyLocation = ContentValidation.NormaliseText(FriendlyLocation, MaximumLocationLength);
            if (string.IsNullOrEmpty(FriendlyLocation)) { issues += "Friendly location cannot be empty. "; }

            // Verify Gathering is now or in the future
            if (HappenedBefore(StartTime, Time - MaximumEarlyBirdStart)) { issues += "Gathering is in the past. "; }

            // If in the past, make it now
            if (HappenedBefore(StartTime, Time)) { StartTime = Time; }

            // Verify Gathering is within a reasonable time
            if (After(StartTime, Time + MaximumCreationAdvance)) { issues += "Gathering is too far in the future. "; }

            // Force degree to be sensible
            DegreeOfPrivacy = Math.Clamp(DegreeOfPrivacy, 1, 3);

            // Verify group bounds
            if (GroupMaximum != 0 &&
                (GroupMaximum <= GroupMinimum ||
                GroupMaximum < 4)) { issues += "Gathering group bounds invalid. "; }

            return issues.Equals("");
        }

        public async Task<List<(User User, GatheringBond State)>> GetCompanionsOf(User user)
        {
            List<(User User, GatheringBond State)> companions = new();

            foreach (var userDetails in await AllUsers)
            {
                if (await user.IsCompanionsWith(userDetails.User))
                {
                    companions.Add(userDetails);
                }
            }

            return companions;
        }

        public async Task<List<PostShard>> GetSnapshotsOf(User user)
        {
            return (await Snapshots).Where(snapshot => snapshot.User.Id.Equals(user.Id)).ToList();
        }

		#endregion

		#region Checks

		public async Task<bool> IsVisibleTo(User user)
		{
			// Note: This is efficient with multiple users. For multiple gatherings, see User.CanView

            // Check if user is host
            if (IsHostedBy(user))
            { return true; }

            // Check if gathering is deleted
            if (IsDeleted)
            { return false; }

			// Check if user account is locked
			if (user.IsLocked)
            { return false; }

			// Check if user's account is limited
			if (!user.CanAttend)
			{
				// User cannot join normal gatherings
                // Check if user can join companion gatherings and Host is companions with the user
				if (!(user.CanAttendCompanions && await (await Owner).IsCompanionsWith(user)))
				{ return false; }
			}

			// Check if user is blocked by or blocking gathering host
			if (await (await Owner).IsBlockedBy(user) || await (await Owner).IsBlocking(user))
			{ return false; }

            // Check if user is within degree of privacy
            if (DegreeOfPrivacy < 3 && !await Terminal.GatheringDirector.RequestUserIsAuthorisedGuest(user, this))
            { return false; }

            return true;
		}

        public async Task<bool> IsJoinableBy(User user)
        {
            // Check if gathering is joinable
            if (!IsOpen)
            { return false; }

            // Check if user can see gathering
            if (!await IsVisibleTo(user))
            { return false; }

            // Check if user is kicked from gathering
            if ((await Kicked).Contains(user))
            { return false; }

            /*
            // Check if user or user's haunt is within a reasonable distance
            if (!GeoLocation.AreInRange(await user.LastKnownLocation, Location, MaximumJoinDistance) &&
                !GeoLocation.AreInRange(await user.Haunt, Location, MaximumJoinDistance))
            { return false; }
            */

            return true;
        }

        public bool IsModifiableBy(User user)
        {
			// Check if user is gathering host
			if (OwnerId.Equals(user.Id))
			{ return true; }

			return false;
        }

        public bool IsHostedBy(User user)
        {
			// Check if user is gathering host
			if (OwnerId.Equals(user.Id))
			{ return true; }

			return false;
        }

        public async Task<bool> HasUserRelationship(User user)
        {
            // Check if user has interacted with gathering
            return IsHostedBy(user) || (await AllUsers).Exists(x => x.User.Id == user.Id);
        }

        public async Task<bool> HasOnGuestList(User user)
        {
            // Check if user is affiliated with the gathering
            return (await Guests).Contains(user) || await WasAttendedBy(user);
        }

        public async Task<bool> WasAttendedBy(User user)
        {
            // Check if user is or was on the guest list
            return (await Arrived).Contains(user) || (await Left).Contains(user);
		}
        
        public async Task<bool> IsInRange(User user)
            => GeoLocation.AreInRange(Location, await user.LastKnownLocation, ArrivalDistance);

        public bool IsTerminable()
        {
            // Ensure gathering is ongoing
            if (!IsOngoing)
            { return false; }

            return true;
        }

        public bool IsCancelable()
        {
            // Ensure gathering has not already occurred
            if (IsOngoing || IsTerminated)
            { return false; }

            return true;
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
            Verify(HasAlready(StartTime) || IsModifiableBy(user),
                new UserErrorException(GatheringErrorCode.NOT_STARTED));

            // Verify user can etch into the gathering
            Verify(await WasAttendedBy(user) || IsModifiableBy(user),
                new UserErrorException(GatheringErrorCode.NOT_GUEST));
		}

		public async Task<bool> Reported()
        {
            // Check if there are enough reports
            if ((await GatheringReports).Count < 3)
            { return false; }

            return true;
        }

        #endregion

        #region Actions

        public async Task<string> NotifyGuests(CardinalNotification notification, DateTimeOffset? notifyAt = null, bool notifyHost = true)
        {
            var targets = (await Guests).Concat(await Arrived).ToList();

            if (!notifyHost)
            {
                targets.Remove(await Owner);
            }

            return await Terminal.NotificationDirector.NotifyUsersAsync(notification, notifyAt, targets.ToArray());
        }

		#endregion

		#region Dissimilation

		public override bool Equals(object obj)
		{
			return obj is Segment other &&
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

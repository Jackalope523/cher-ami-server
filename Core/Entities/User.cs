using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Notifications;

using static Core.Entities.Psijic;
using static Core.Entities.Arbiter;

namespace Core.Entities
{
    using static CoreTerminal;

    internal class User
    {
        #region Olive Branches

        public static async Task<string> NotifyAll(CanaryNotification notification, DateTimeOffset? notifyAt = null, params User[] users)
        {
            return await Terminal.NotificationDirector.NotifyUsersAsync(notification, notifyAt, users);
        }

        public static async Task<string> NotifyAll(CanaryNotification notification, params User[] users)
        {
            return await NotifyAll(notification, null, users);
        }

        #endregion

        #region Variables

        //////
        // Constants
        //////////////

        public const int MaximumReputation = 100;
        public const int ReputationPopulation = 20;
        public const float ReputationIntensity = 2.2f;

        public readonly static TimeSpan DuplicateReportFrequency = TimeSpan.FromDays(14);

        public static User Redacted
            => new() { Id = 0 };

        public static User Hidden
            => new() { Id = -1, Name = "Hidden User" };

        public static User Hollow
            => new() { Id = -2 };

        ///////
        // Properties
        ///////////////

        public long Id { get; init; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTimeOffset DateOfBirth { get; init; }

        public DateTimeOffset JoinDate { get; init; }
        public int Reputation { get; set; }

        public bool IsPhoneConfirmed { get; set; }
        public bool IsEmailConfirmed { get; set; }

        public bool IsDeleted { get; set; }
        public string SecurityStamp { get; set; }
        public DateTimeOffset? LockoutDate { get; set; }
        public int AccessTries { get; set; }
        public DateTimeOffset TimeOfUserAgreement { get; set; }
        public Guid NotificationId { get; set; } = Guid.Empty;

        public UserAccountStatus AccountStatus { get; set; }
        public bool CanAttend => AccountStatus == UserAccountStatus.Active ||
            AccountStatus == UserAccountStatus.Impotent;
        public bool CanAttendCompanions => CanAttend ||
            AccountStatus == UserAccountStatus.Limited;
        public bool CanHost => AccountStatus == UserAccountStatus.Active;
        public bool IsLocked => AccountStatus == UserAccountStatus.Blacklisted;

        public CharacterVector Character { get; set; }

        ////////
        // Synced Properties
        //////////////////////

        public Synced<NotificationProfile> NotificationProfile { get; }

        private Synced<(GeoLocation Location, Distance Radius)> LocationSync { get; }
		public Synced<GeoLocation> LastKnownLocation { get; }
        public Synced<Distance> LastKnownRadius { get; }

        private Synced<(GeoLocation Location, Distance Radius, int Stability)> HauntSync { get; }
        public Synced<GeoLocation> Haunt { get; }
        public Synced<Distance> HauntRadius { get; }
        public Synced<int> HauntStability { get; }

        public Synced<List<Gathering>> PastGatherings { get; }
        public Synced<List<Gathering>> OngoingGatherings { get; }
        public Synced<List<Gathering>> UpcomingGatherings { get; }

        public Synced<List<User>> Companions { get; }
        public Synced<List<User>> Following { get; }
        public Synced<List<User>> Followers { get; }
        public Synced<List<User>> Blocking { get; }
        public Synced<List<User>> BlockedBy { get; }

        public Synced<List<PenaltyShard>> Penalties { get; }

        private Synced<(List<UserReport> UserReports, List<GatheringReport> GatheringReports, List<SnapshotReport> SnapshotReports)> ReportsSync { get; }
        public Synced<List<UserReport>> Reports { get; }
        public Synced<List<GatheringReport>> GatheringReports { get; }
        public Synced<List<SnapshotReport>> SnapshotReports { get; }

        public Synced<List<string>> Connections { get; }

        public Synced<List<(Conversation Conversation, CoreMembership Membership)>> Conversations { get; }

        #endregion

        #region Initialisation & Extraction

        public static async Task<User> GetUserAsync(long id)
        {
            return new(await Terminal.AccountDatabase.FindUserByIdAsync(id));
        }

        public User()
        {
            NotificationProfile = new(() => Terminal.NotificationDirector.RequestNotificationProfileAsync(this));

            LocationSync = new(() => Terminal.AccountDirector.RequestLastKnownUserLocationAsync(this));
            LastKnownLocation = new(async () => (await LocationSync.Value().ConfigureAwait(false)).Location);
            LastKnownRadius = new(async () => (await LocationSync.Value().ConfigureAwait(false)).Radius);

            HauntSync = new(() => Terminal.AccountDirector.RequestUserHauntAsync(this));
            Haunt = new(async () => (await HauntSync.Value().ConfigureAwait(false)).Location);
            HauntRadius = new(async () => (await HauntSync.Value().ConfigureAwait(false)).Radius);
            HauntStability = new(async () => (await HauntSync.Value().ConfigureAwait(false)).Stability);

            PastGatherings = new(() => Terminal.GatheringDirector.RequestPastGatheringsForUserAsync(this));
            OngoingGatherings = new(() => Terminal.GatheringDirector.RequestOngoingGatheringsForUserAsync(this));
            UpcomingGatherings = new(() => Terminal.GatheringDirector.RequestUpcomingGatheringsForUserAsync(this));

            Companions = new(() => Terminal.NestDirector.RequestCompanionsAsync(this));
            Following = new(() => Terminal.NestDirector.RequestFollowedUsersAsync(this));
            Followers = new(() => Terminal.NestDirector.RequestFollowersAsync(this));
            Blocking = new(() => Terminal.NestDirector.RequestBlockedUsersAsync(this));
            BlockedBy = new(() => Terminal.NestDirector.RequestUsersBlockingAsync(this));

            Penalties = new(() => Terminal.DisciplineDirector.RequestPenaltiesForUserAsync(this));

            ReportsSync = new(() => Terminal.DisciplineDirector.RequestAllReportsAsync(this));
            Reports = new(async () => (await ReportsSync.Value().ConfigureAwait(false)).UserReports);
            GatheringReports = new(async () => (await ReportsSync.Value().ConfigureAwait(false)).GatheringReports);
            SnapshotReports = new(async () => (await ReportsSync.Value().ConfigureAwait(false)).SnapshotReports);

            Connections = new(() => Terminal.ConnectionDirector.RequestUserConnectionsAsync(this));

            Conversations = new(() => Terminal.MessageDirector.RequestConversationsForUserAsync(this));
        }

        public User(CoreUser fromUser) : this()
        {
            Id = fromUser.Id;
            PhoneNumber = fromUser.PhoneNumber;
            Email = fromUser.Email;
            Name = fromUser.Name;
            Code = fromUser.Code;
            DateOfBirth = fromUser.DateOfBirth;
            JoinDate = fromUser.JoinDate;
            Reputation = fromUser.Reputation;
            IsPhoneConfirmed = fromUser.IsPhoneConfirmed;
            IsEmailConfirmed = fromUser.IsEmailConfirmed;
            IsDeleted = fromUser.IsPendingDeletion;
            SecurityStamp = fromUser.SecurityStamp;
            LockoutDate = fromUser.LockoutDate;
            AccessTries = fromUser.AccessTries;
            AccountStatus = fromUser.AccountStatus;
            Character = new(fromUser.Character);
            TimeOfUserAgreement = fromUser.TimeOfUserAgreement;
            NotificationId = fromUser.NotificationId;
        }

        public CoreUser ToCoreUser()
        {
            return new(Id, PhoneNumber, Email, Name, Code, DateOfBirth,
                IsPhoneConfirmed, IsEmailConfirmed, IsDeleted,
                SecurityStamp, LockoutDate, AccessTries, AccountStatus,
                JoinDate, Reputation,
                Character.ToCharacter(), TimeOfUserAgreement,
                NotificationId);
        }

        public AccountShard ToAccountShard()
        {
            return new(Id, PhoneNumber, Email, Name, Code, DateOfBirth,
                IsPhoneConfirmed, IsEmailConfirmed, AccountStatus,
                JoinDate, TimeOfUserAgreement, NotificationId);
        }

        public UserShard ToUserShard()
        {
            return new(Id, Name);
        }

		#endregion

		#region Composition

		public bool ValidateAndNormalise(out string issues)
        {
            issues = "";

            // Verify phone number
            if (!ContentValidation.TryNormalisePhoneNumber(PhoneNumber, out string normalisedPhoneNumber))
            { issues += "Invalid phone number. "; }

            // Verify email if it exists
            if (!string.IsNullOrEmpty(Email) &&
                !ContentValidation.IsEmailValid(Email)) { issues += "Invalid email. "; }

            // Verify user age
            if (HasYet(DateOfBirth + (OneYear * 18))) { issues += "User is too young. "; }

            // Normalise
            Email = string.IsNullOrEmpty(Email) ? Email : Email.ToLower();
            PhoneNumber = normalisedPhoneNumber;

            return issues.Equals("");
        }

        public int GetAge()
        {
            int age = Time.Year - DateOfBirth.Year;

            if (Time < DateOfBirth.AddYears(age))
            {
                age--;
            }

            return age;
        }

        public void GenerateSecurityStamp()
        {
            SecurityStamp = Convert.ToBase64String(RandomNumberGenerator.GetBytes(20));
        }

		public async Task CalculateReputation()
        {
            _ = (Penalties.Sync(), Followers.Sync());

            // Get all recent penalties
            var penalties = (await Penalties).Where(penalty => HasYet(penalty.TimeOfPenalty + OneYear)).ToList();
            int follows = (await Followers).Count;
            int reputationRaw = Math.Clamp(follows, -ReputationPopulation, ReputationPopulation);

            float normal = MathF.Tan(ReputationIntensity / 2) / ReputationPopulation;

            Reputation = (int) (MathF.Atan(reputationRaw * normal) * (MaximumReputation / ReputationIntensity) + (MaximumReputation / 2));
        }

        public void CalculateCharacter(Gathering gatheringAttended, TimeSpan timeAttended)
        {
            // Modified by time spent
            float modifier = (float) (Math.Log10(3 * timeAttended.TotalMinutes + 3) / 15d);

            Character = Character.MoveTowards(gatheringAttended.Character, modifier);
        }

        public async Task<Gathering> NextGathering()
        {
            var upcoming = await UpcomingGatherings;
            upcoming.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return upcoming.Count != 0 ? upcoming.First() : Gathering.None;
        }

        public async Task<Gathering> LastGathering()
        {
            var previous = await PastGatherings;
            previous.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return previous.Count != 0 ? previous.Last() : Gathering.None;
        }

		#endregion

		#region Checks

        public async Task<bool> IsNeutralOrUnrequitedWith(User otherUser)
        {
            return !await IsFollowing(otherUser) &&
                !await IsBlockedBy(otherUser) &&
                !await IsBlocking(otherUser);
        }

        public async Task<bool> IsCompanionsWith(User otherUser)
		{
			// Check if users are companions
			if ((await Companions).Contains(otherUser))
            { return true; }

            return false;
        }

        public async Task<bool> IsFollowing(User otherUser)
        {
			// Check if user is following target
			if ((await Following).Contains(otherUser))
			{ return true; }

            return false;
        }

        public async Task<bool> IsBlocking(User otherUser)
        {
			// Check if user is blocking target
			if ((await Blocking).Contains(otherUser))
			{ return true; }

            return false;
        }

		public async Task<bool> IsBlockedBy(User otherUser)
		{
			// Check if user is blocked by target
			if ((await BlockedBy).Contains(otherUser))
			{ return true; }

			return false;
		}

        public async Task<bool> IsAtGathering()
        {
            return (await OngoingGatherings).Count > 0;
        }

        public async Task<bool> IsOnline()
        {
            return (await Connections).Count > 0;
        }

		public async Task<bool> CanView(Gathering gathering)
		{
            // Note: This is efficient with multiple gatherings. For multiple users, see Gathering.IsVisibleTo

            // Check if user is host
            if (gathering.IsHostedBy(this))
            { return true; }

            // Check if gathering is deleted
            if (gathering.IsDeleted)
            { return false; }

			// Check if user account is locked
			if (IsLocked)
			{ return false; }

			// Check if user's account is limited
			if (!CanAttend)
			{
                // User cannot join normal gatherings
                // Check if user can join companion gatherings and Host is companions with the user
				if (!(CanAttendCompanions && await IsCompanionsWith(await gathering.Host)))
				{ return false; }
			}

            // Check if user is blocked by or blocking gathering host
            if (await IsBlockedBy(await gathering.Host) || await IsBlocking(await gathering.Host))
			{ return false; }

            // Check if user is within degree of privacy
            if (gathering.DegreeOfPrivacy < 3 && !await Terminal.GatheringDirector.RequestUserIsAuthorisedGuest(this, gathering))
            { return false; }

			return true;
		}

		public async Task<bool> CanJoin(Gathering gathering)
		{
			// Check if gathering is joinable
			if (!gathering.IsOpen)
			{ return false; }

			// Check if user can see gathering
			if (!await CanView(gathering))
			{ return false; }

            // Check if user is kicked from gathering
            if ((await gathering.Kicked).Contains(this))
            { return false; }

            /*
			// Check if user or user's haunt is within a reasonable distance
			if (!GeoLocation.AreInRange(await LastKnownLocation, gathering.Location, gathering.MaximumJoinDistance) &&
				!GeoLocation.AreInRange(await Haunt, gathering.Location, gathering.MaximumJoinDistance))
			{ return false; }
            */

            return true;
		}

		public async Task<bool> CanCheckIn(Gathering gathering)
		{
			// Check if currently at another gathering
			if (await IsAtGathering())
			{ return false; }

			// Check if user is incoming to the gathering
			if ((await NextGathering()).Equals(gathering))
			{ return false; }

            // Check that gathering is ongoing
            if (!gathering.IsOngoing)
			{ return false; }

            // Check if user is in range of the gathering
			// if (!GeoLocation.AreInRange(await LastKnownLocation, gathering.Location, Gathering.MaximumJoinDistance))
			// { return false; }

            return true;
		}

        public async Task CanEtch(Gathering gathering)
		{
			// Verify user can etch into the gathering
			Verify(await gathering.HasOnGuestList(this) || gathering.IsModifiableBy(this),
				new UserErrorException(GatheringErrorCode.NOT_GUEST));
		}

		public bool Taken(SnapshotShard snapshot)
        {
            return snapshot.User.Id.Equals(Id);
		}

        public async Task<bool> CanReport()
        {
            var recentReportCount = (await Reports).Count(report => After(report.ReportTime, Time - FifteenMinutes))
                + (await GatheringReports).Count(report => After(report.ReportTime, Time - FifteenMinutes));

            if (recentReportCount > 10)
            { return false; }

            return true;
        }

        public async Task<bool> CanReport(User otherUser, UserReportType reportType)
        {
            var availableReports = await AvailableReportTypes(otherUser);

            return availableReports.Contains(reportType);
        }

        public async Task<List<UserReportType>> AvailableReportTypes(User otherUser)
        {
            // Gather recent reports by user against target 
            var reportedTypesByUser = (await otherUser.Reports)
                .Where(report => report.ReportingUserId.Equals(Id) &&
                Psijic.HappenedBefore(Time - DuplicateReportFrequency, report.ReportTime))
                .Select(report => report.ReportType);

            var reportTypes = Enum.GetValues<UserReportType>().ToList();

            var availableReportTypes = reportTypes.Except(reportedTypesByUser);

            // Return exclusion
            return availableReportTypes.ToList();
        }

        public async Task<bool> CanReport(Gathering gathering, GatheringReportType reportType)
        {
            var availableReports = await AvailableReportTypes(gathering);

            return availableReports.Contains(reportType);
        }

        public async Task<List<GatheringReportType>> AvailableReportTypes(Gathering gathering)
        {
            // Gather recent reports by user against target 
            var reportedTypesByUser = (await gathering.GatheringReports)
                .Where(report => report.ReportingUserId.Equals(Id))
                .Select(report => report.ReportType);

            var reportTypes = Enum.GetValues<GatheringReportType>().ToList();

            var availableReportTypes = reportTypes.Except(reportedTypesByUser);

            // Return exclusion
            return availableReportTypes.ToList();
        }

        public async Task<bool> CanReport(SnapshotShard snapshot, User snapshotAuthor, SnapshotReportType reportType)
        {
            var availableReports = await AvailableReportTypes(snapshot, snapshotAuthor);

            return availableReports.Contains(reportType);
        }

        public async Task<List<SnapshotReportType>> AvailableReportTypes(SnapshotShard snapshot, User snapshotAuthor)
        {
            // Gather recent reports by user against target 
            var reportedTypesByUser = (await snapshotAuthor.SnapshotReports)
                .Where(report => report.ReportedSnapshotId == snapshot.Id && report.ReportingUserId.Equals(Id))
                .Select(report => report.ReportType);

            var reportTypes = Enum.GetValues<SnapshotReportType>().ToList();

            var availableReportTypes = reportTypes.Except(reportedTypesByUser);

            // Return exclusion
            return availableReportTypes.ToList();
        }

        public async Task<bool> CanFollow(User target, bool hasCode = false)
        {
            // Check if already following user
            if (await target.IsFollowing(this))
            {
                return true;
            }

            bool blockFollow = await IsBlocking(target) || await IsBlockedBy(target);

            // Check if code bypass
            if (hasCode)
            { return !blockFollow; }

            var haveMutualGathering = await Terminal.NestDirector.RequestAttendedMutualGatheringAsync(this, target);

            return !blockFollow && haveMutualGathering;
        }

		#endregion

		#region Effects
        
        public async Task HandleHaunt()
        {
            // Check if user has a haunt and recent location
            if (!(await Haunt).Exists || !(await LastKnownLocation).Exists)
            {
                Haunt.Set(await LastKnownLocation);
                HauntStability.Set(1);
                return;
            }

            // Check if recent location is within haunt area
            if (GeoLocation.DistanceBetween(await Haunt, await LastKnownLocation) < await HauntRadius)
            {
                HauntStability.Set(HauntStability + 1);
            }
            else
            {
                HauntStability.Set(HauntStability - 1);

                // If our haunt is unstable, move it
                if (await HauntStability < 0)
                {
                    HauntStability.Set(0);
                    Haunt.Set(await LastKnownLocation);
                }
            }
        }

        public async Task Penalised()
            => await CalculateReputation();

		public async Task<UserAccountStatus> GatheringReported()
        {
			// Check if there are enough reports
			if ((await GatheringReports).Count < 4)
			{ return AccountStatus; }

			return UserAccountStatus.Impotent;
        }

        public async Task<UserAccountStatus> Reported()
        {
            var currentStatus = AccountStatus;
            UserAccountStatus nextStatus;

			// Check if there are enough reports
			if ((await Reports).Count < 4)
			{ return AccountStatus; }
			else if ((await Reports).Count < 6)
			{ nextStatus = UserAccountStatus.Limited; }
            else if ((await Reports).Count < 10)
			{ nextStatus = UserAccountStatus.Suspended; }
            else
            { nextStatus = UserAccountStatus.Blacklisted; }

            // Notify user of change
            if (!currentStatus.Equals(nextStatus))
            { } // todo

            return nextStatus;
        }

		#endregion

		#region Actions

        public async Task<string> Notify(CanaryNotification notification, DateTimeOffset? notifyAt = null)
        {
             return await Terminal.NotificationDirector.NotifyUserAsync(this, notification, notifyAt);
        }

        public async Task<string> NotifyFollowers(CanaryNotification notification, DateTimeOffset? notifyAt = null)
        {
            return await Terminal.NotificationDirector.NotifyUsersAsync(notification, notifyAt, (await Followers).ToArray());
        }

        public async Task<string> NotifyCompanions(CanaryNotification notification, DateTimeOffset? notifyAt = null)
        {
            return await Terminal.NotificationDirector.NotifyUsersAsync(notification, notifyAt, (await Companions).ToArray());
        }

		#endregion

		#region Dissimilation

		public override bool Equals(object obj)
		{
			return obj is User other && Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		#endregion
	}
}

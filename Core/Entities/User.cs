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

        public static async Task<string> NotifyAll(CardinalNotification notification, DateTimeOffset? notifyAt = null, params User[] users)
        {
            return await Terminal.NotificationDirector.NotifyUsersAsync(notification, notifyAt, users);
        }

        public static async Task<string> NotifyAll(CardinalNotification notification, params User[] users)
        {
            return await NotifyAll(notification, null, users);
        }

        #endregion

        #region Variables

        //////
        // Constants
        //////////////

        public readonly static TimeSpan DuplicateReportFrequency = TimeSpan.FromDays(14);

        public static User Redacted
            => new() { Id = 0 };

        public static User Hidden
            => new() { Id = -1, GivenName = "Hidden User" };

        public static User Hollow
            => new() { Id = -2 };

        ///////
        // Properties
        ///////////////

        public long Id { get; init; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string NormalisedEmail { get; set; }
        public string Title { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public DateTimeOffset DateOfBirth { get; init; }

        public DateTimeOffset JoinDate { get; init; }

        public bool IsPhoneConfirmed { get; set; }
        public bool IsEmailConfirmed { get; set; }

        public bool IsDeleted { get; set; }
        public string SecurityStamp { get; set; }
        public DateTimeOffset? LockoutDate { get; set; }
        public int AccessTries { get; set; }
        public DateTimeOffset TimeOfUserAgreement { get; set; }
        public Guid NotificationId { get; set; } = Guid.Empty;

        public UserAccountStatus AccountStatus { get; set; }
        public bool CanPost => AccountStatus == UserAccountStatus.Active;
        public bool IsLocked => AccountStatus == UserAccountStatus.Blacklisted;

        ////////
        // Synced Properties
        //////////////////////

        public Synced<NotificationProfile> NotificationProfile { get; }

        public Synced<List<Circle>> Circles { get; }

        public Synced<CorePaymentMethod> PaymentMethod { get; }

        public Synced<List<User>> Blocking { get; }
        public Synced<List<User>> BlockedBy { get; }

        private Synced<(List<UserReport> UserReports, List<PostReport> PostReports)> ReportsSync { get; }
        public Synced<List<UserReport>> UserReports { get; }
        public Synced<List<PostReport>> PostReports { get; }

        public Synced<List<string>> Connections { get; }

        public Synced<List<(Chat Conversation, CoreMembership Membership)>> Conversations { get; }

        #endregion

        #region Initialisation & Extraction

        public static async Task<User> GetUserAsync(long id)
        {
            return new(await Terminal.AccountDatabase.GetUserByIdAsync(id));
        }

        public User()
        {
            NotificationProfile = new(() => Terminal.NotificationDirector.RequestNotificationProfileAsync(this));

            Circles = new(() => Terminal.CircleDirector.RequestUpcomingGatheringsForUserAsync(this));
            PaymentMethod = new(() => Terminal);

            Blocking = new(() => Terminal.ProfileDirector.RequestBlockedUsersAsync(this));
            BlockedBy = new(() => Terminal.ProfileDirector.RequestUsersBlockingAsync(this));

            ReportsSync = new(() => Terminal.ReportDirector.RequestAllReportsAsync(this));
            UserReports = new(async () => (await ReportsSync.Value().ConfigureAwait(false)).UserReports);
            PostReports = new(async () => (await ReportsSync.Value().ConfigureAwait(false)).PostReports);

            Connections = new(() => Terminal.ConnectionDirector.RequestUserConnectionsAsync(this));

            Conversations = new(() => Terminal.ChatDirector.RequestChatsForUserAsync(this));
        }

        public User(CoreUser fromUser) : this()
        {
            Id = fromUser.Id;
            PhoneNumber = fromUser.PhoneNumber;
            Email = fromUser.Email;
            Email = fromUser.Email;
            Title = fromUser.Title;
            GivenName = fromUser.GivenName;
            FamilyName = fromUser.FamilyName;
            DateOfBirth = fromUser.DateOfBirth;
            JoinDate = fromUser.JoinDate;
            IsPhoneConfirmed = fromUser.IsPhoneConfirmed;
            IsEmailConfirmed = fromUser.IsEmailConfirmed;
            IsDeleted = fromUser.IsPendingDeletion;
            SecurityStamp = fromUser.SecurityStamp;
            LockoutDate = fromUser.LockoutDate;
            AccessTries = fromUser.AccessTries;
            AccountStatus = fromUser.AccountStatus;
            TimeOfUserAgreement = fromUser.TimeOfUserAgreement;
            NotificationId = fromUser.NotificationId;
        }

        public CoreUser ToCoreUser()
        {
            return new(Id, PhoneNumber, Email, NormalisedEmail, Title, GivenName, FamilyName, DateOfBirth,
                IsPhoneConfirmed, IsEmailConfirmed, IsDeleted,
                SecurityStamp, LockoutDate, AccessTries, AccountStatus,
                JoinDate, TimeOfUserAgreement, NotificationId);
        }

        public AccountShard ToAccountShard()
        {
            return new(Id, PhoneNumber, Email, Title, GivenName, FamilyName, DateOfBirth,
                IsPhoneConfirmed, IsEmailConfirmed, AccountStatus,
                JoinDate, TimeOfUserAgreement, NotificationId);
        }

        public UserShard ToUserShard()
        {
            return new(Id, GivenName, FamilyName);
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
            if (HasYet(DateOfBirth + (OneYear * 13))) { issues += "User is too young. "; }

            // Normalise
            NormalisedEmail = string.IsNullOrEmpty(Email) ? Email : Email.ToLower();
            PhoneNumber = normalisedPhoneNumber;

            return issues.Equals("");
        }

        public void GenerateSecurityStamp()
        {
            SecurityStamp = Convert.ToBase64String(RandomNumberGenerator.GetBytes(20));
        }

		#endregion

		#region Checks

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

        public async Task<bool> IsOnline()
        {
            return (await Connections).Count > 0;
        }

		public async Task<bool> CanView(Issue gathering)
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

			return true;
		}

        public async Task CanPostTo(Issue issue)
		{
			Verify(await issue.HasOnGuestList(this),
				new UserErrorException(CircleErrorCode.NOT_GUEST));
		}

		public bool Owns(PostShard post)
        {
            return post.UserId.Equals(Id);
		}

        public async Task<bool> CanReport()
        {
            var recentReportCount = (await UserReports).Count(report => After(report.ReportTime, Time - FifteenMinutes))
                + (await PostReports).Count(report => After(report.ReportTime, Time - FifteenMinutes));

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
            var reportedTypesByUser = (await otherUser.UserReports)
                .Where(report => report.ReportingUserId.Equals(Id) &&
                Psijic.HappenedBefore(Time - DuplicateReportFrequency, report.ReportTime))
                .Select(report => report.ReportType);

            var reportTypes = Enum.GetValues<UserReportType>().ToList();

            var availableReportTypes = reportTypes.Except(reportedTypesByUser);

            // Return exclusion
            return availableReportTypes.ToList();
        }

        public async Task<bool> CanReport(PostShard post, User postAuthor, PostReportType reportType)
        {
            var availableReports = await AvailableReportTypes(post, postAuthor);

            return availableReports.Contains(reportType);
        }

        public async Task<List<PostReportType>> AvailableReportTypes(PostShard post, User postAuthor)
        {
            // Gather recent reports by user against target 
            var reportedTypesByUser = (await postAuthor.PostReports)
                .Where(report => report.ReportedPostId == post.Id && report.ReportingUserId.Equals(Id))
                .Select(report => report.ReportType);

            var reportTypes = Enum.GetValues<PostReportType>().ToList();

            var availableReportTypes = reportTypes.Except(reportedTypesByUser);

            // Return exclusion
            return availableReportTypes.ToList();
        }

        public async Task<bool> CanMessage(User target)
        {
            bool blocked = await IsBlocking(target) || await IsBlockedBy(target);

            return !blocked;
        }

		#endregion

		#region Effects

		public async Task<UserAccountStatus> PostReported()
        {
			// Check if there are enough reports
			if ((await PostReports).Count < 4)
			{ return AccountStatus; }

			return UserAccountStatus.Limited;
        }

        public async Task<UserAccountStatus> Reported()
        {
            var currentStatus = AccountStatus;
            UserAccountStatus nextStatus;

			// Check if there are enough reports
			if ((await UserReports).Count < 4)
			{ return AccountStatus; }
			else if ((await UserReports).Count < 6)
			{ nextStatus = UserAccountStatus.Limited; }
            else if ((await UserReports).Count < 10)
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

        public async Task<string> Notify(CardinalNotification notification, DateTimeOffset? notifyAt = null)
        {
             return await Terminal.NotificationDirector.NotifyUserAsync(this, notification, notifyAt);
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

    internal class CircleMember : User
    {
        public DateTimeOffset DateJoined { get; set; }
        public CircleMembershipType MembershipType { get; set; }

        public static async Task<CircleMember> GetMemberAsync(long id)
        {
            return new(await Terminal.AccountDatabase.GetUserByIdAsync(id));
        }

        public static async Task<CircleMember> FromMembershipAsync(CoreCircleMembership membership)
        {
            CircleMember user = new(await Terminal.AccountDatabase.GetUserByIdAsync(membership.UserId))
            {
                DateJoined = membership.DateJoined,
                MembershipType = membership.Type
            };

            return user;
        }

        public CircleMembershipShard ToCircleMembershipShard()
        {
            return new(Id, DateJoined, MembershipType);
        }
    }
}

using Core.Boundaries;
using Core.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static Core.Entities.Psijic;

namespace Core.Entities
{
    using static CoreTerminal;

    public class User
    {
        public static async Task<string> NotifyAll(CardinalNotification notification, DateTimeOffset? notifyAt = null, params User[] users)
        {
            return await Terminal.NotificationDirector.NotifyUsersAsync(notification, notifyAt, users);
        }

        public static async Task<string> NotifyAll(CardinalNotification notification, params User[] users)
        {
            return await NotifyAll(notification, null, users);
        }

        //////
        // Constants
        //////////////

        public readonly static TimeSpan DuplicateReportFrequency = TimeSpan.FromDays(14);

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
        public DateTimeOffset DateOfBirth { get; set; }

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

        public NotificationProfile NotificationProfile { get; }

        public List<CoreCircle> Circles { get; }

        public CorePaymentMethod PaymentMethod { get; }

        public List<User> Blocking { get; }
        public List<User> BlockedBy { get; }

        public List<UserReport> UserReports { get; }
        public List<PostReport> PostReports { get; }

        #region Initialisation & Extraction

        public static async Task<User> GetUserAsync(long id)
        {
            return new(await Terminal.AccountDatabase.GetUserByIdAsync(id));
        }

        public User()
        { 
        }

        public User(NotificationProfile notificationProfile, List<CoreCircle> circles, CorePaymentMethod paymentMethod, List<User> blocking, List<User> blockedBy, List<UserReport> userReports, List<PostReport> postReports)
        {
            NotificationProfile = notificationProfile;

            Circles = circles;
            PaymentMethod = paymentMethod;

            Blocking = blocking;
            BlockedBy = blockedBy;

            UserReports = userReports;
            PostReports = postReports;
        }

        public User(CoreUser fromUser)
        {
            Id = fromUser.Id;
            PhoneNumber = fromUser.PhoneNumber;
            Email = fromUser.Email;
            NormalisedEmail = fromUser.NormalisedEmail;
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
			if (Blocking.Contains(otherUser))
			{ return true; }

            return false;
        }

		public async Task<bool> IsBlockedBy(User otherUser)
		{
			// Check if user is blocked by target
			if (BlockedBy.Contains(otherUser))
			{ return true; }

			return false;
		}

        public async Task<bool> CanView(CoreCircle circle)
        {
            // Note: This is efficient with multiple circles/issues. For multiple users, see Circle.IsVisibleTo

            // Check if user is admin
            if (await circle.IsModifiableBy(this))
            { return true; }

            // Check if circle is deleted
            if (circle.IsDeleted)
            { return false; }

            // Check if user account is locked
            if (IsLocked)
            { return false; }

            // Check if user is member
            if (!await circle.HasMember(this))
            { return true; }

            return false;

        }

		public async Task<bool> CanView(Issue issue)
		{
            return await CanView(issue.Circle);
		}

        public async Task<bool> CanPostTo(CoreCircle circle)
		{
            return await circle.HasMember(this);
		}

        public async Task<bool> CanPostTo(Issue issue)
		{
            return await CanPostTo(issue.Circle);
		}

		public bool Owns(PostShard post)
        {
            return post.UserId.Equals(Id);
		}

        public async Task<bool> CanReport()
        {
            var recentReportCount = UserReports.Count(report => After(report.ReportTime, Time - FifteenMinutes))
                + PostReports.Count(report => After(report.ReportTime, Time - FifteenMinutes));

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
            var reportedTypesByUser = otherUser.UserReports
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
            var reportedTypesByUser = postAuthor.PostReports
                .Where(report => report.ReportedPostId == post.Id && report.ReportingUserId.Equals(Id))
                .Select(report => report.ReportType);

            var reportTypes = Enum.GetValues<PostReportType>().ToList();

            var availableReportTypes = reportTypes.Except(reportedTypesByUser);

            // Return exclusion
            return availableReportTypes.ToList();
        }

		#endregion

		#region Effects

		public async Task<UserAccountStatus> PostReported()
        {
			// Check if there are enough reports
			if (PostReports.Count < 4)
			{ return AccountStatus; }

			return UserAccountStatus.Limited;
        }

        public async Task<UserAccountStatus> Reported()
        {
            var currentStatus = AccountStatus;
            UserAccountStatus nextStatus;

			// Check if there are enough reports
			if (UserReports.Count < 4)
			{ return AccountStatus; }
			else if (UserReports.Count < 6)
			{ nextStatus = UserAccountStatus.Limited; }
            else if (UserReports.Count < 10)
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

    public class CircleMember : User
    {
        public DateTimeOffset DateJoined { get; set; }
        public CircleMembershipType MembershipType { get; set; }

        private CircleMember(CoreUser fromUser) : base(fromUser)
        {
        }

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

        public static CircleMember FromComplete(CoreUser user, CoreCircleMembership membership)
        {
            CircleMember member = new(user)
            {
                DateJoined = membership.DateJoined,
                MembershipType = membership.Type
            };

            return member;
        }

        public CircleMembershipShard ToCircleMembershipShard()
        {
            return new(Id, DateJoined, MembershipType);
        }
    }
}

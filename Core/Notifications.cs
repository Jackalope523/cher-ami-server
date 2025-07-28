using System;
using System.Reflection;
using Core.Boundaries;

namespace Core.Notifications
{
    public enum NotificationGroup
    {
        None,
        IssuePosts,
        IssueReminders,
    }

    public static class NotificationGroupExtensions
    {
        public static bool HasEnabled(this NotificationGroup group, NotificationProfile profile)
        {
            return group switch
            {
                NotificationGroup.None => true,
                NotificationGroup.IssuePosts => profile.IssuePosts,
                NotificationGroup.IssueReminders => profile.IssueReminders,
                _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
            };
        }
    }

    public interface IDeepLink
    {
        public static string BasePath => "almostcanary://";

        public string RelativePath { get; }

        public static string ParseOption(string option, bool? value)
        {
            if (value == null || !value.HasValue)
            { return ""; }

            return value.Value ? $"{option}=true&" : $"{option}=false&";
        }

        public static string ParseOption(string option, string value)
        {
            if (value == null)
            { return ""; }

            return $"{option}={value}&";
        }

        public static string ParseOption(string option, object value)
        {
            if (value == null)
            { return ""; }

            return $"{option}={value}&";
        }

        public static string ParseOption<T>(string option, T? value) where T : struct
        {
            if (value == null || !value.HasValue)
            { return ""; }

            return $"{option}={value.Value}&";
        }

        public static string FormatPath(string path, string options = "")
        {
            if (string.IsNullOrEmpty(options))
            {
                return $"{BasePath}{path}";
            }
            else
            {
                return $"{BasePath}{path}?{options.Remove(options.Length - 1)}";
            }
        }
    }

    public struct CircleDeepLink : IDeepLink
    {
        public enum FocusTarget
        {
            guestlist,
            gallery,
        }

        public string RelativePath { get; private set; }

        public CircleDeepLink(long circleId,
            FocusTarget? focus = null, string invitedBy = null)
        {
            string path = $"circle/{circleId}";
            
            string options = "";

            options += IDeepLink.ParseOption("focus", focus);
            options += IDeepLink.ParseOption("invited_by", invitedBy);

            RelativePath = IDeepLink.FormatPath(path, options);
        }
    }

    public struct IssueDeepLink : IDeepLink
    {
        public string RelativePath { get; private set; }

        public IssueDeepLink(long userId)
        {
            string path = $"profile/{userId}";

            string options = "";

            RelativePath = IDeepLink.FormatPath(path, options);
        }
    }

    public partial class CardinalNotification
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Body { get; set; }
        public string AppUrl { get; set; }
        public string ThreadId { get; set; }

        public NotificationGroup Group { get; set; }

        protected CardinalNotification(string title, string body, IDeepLink deepLink = null, string threadId = "")
        {
            Title = title;
            Body = body;
            AppUrl = deepLink != null ? deepLink.RelativePath : "";
            ThreadId = threadId;
            Group = NotificationGroup.None;
        }

        protected CardinalNotification(string title, string subtitle, string body, IDeepLink deepLink = null, string threadId = "")
            : this(title, body, deepLink, threadId)
        {
            Subtitle = subtitle;
        }

        public bool CheckEnabled(NotificationProfile profile)
        {
            return Group.HasEnabled(profile);
        }
    }

    //////
    // Issue Posts
    ////////////////

    public partial class CardinalNotification
    {
        protected static CardinalNotification IssuePost(CardinalNotification notification)
        {
            notification.Group = NotificationGroup.IssuePosts;
            return notification;
        }

        public static CardinalNotification UserPosted(UserShard addingUser)
            => IssuePost(new("Companion Request",
                $"{addingUser.GivenName} posted to the issue.",
                new IssueDeepLink(addingUser.Id),
                "1"));

        public static CardinalNotification CompanionshipForged(UserShard addingUser)
            => IssuePost(new("New Companion",
                $"Companionship forged with {addingUser.Name} accepted.",
                new IssueDeepLink(addingUser.Id),
                "1"));

        public static CardinalNotification GatheringInvitation(UserShard invitingUser, GatheringShard gathering)
            => IssuePost(new("Gathering Invitation",
                $"{invitingUser.Name} invited you to {gathering.Title}.",
                new GatheringDeepLink(gathering.Id, invitedBy: invitingUser.Name),
                $"{gathering.Id}:1"));
    }

    ///////
    // Issue Reminders
    ////////////////////

    public partial class CardinalNotification
    {
        protected static CardinalNotification IssueReminder(CardinalNotification notification)
        {
            notification.Group = NotificationGroup.IssueReminders;
            return notification;
        }

        public static CardinalNotification CompanionJoined(UserShard companion, GatheringShard gathering)
            => IssueReminder(new(gathering.Title,
                $"{companion.Name} joined the gathering.",
                new GatheringDeepLink(gathering.Id, focus: CircleDeepLink.FocusTarget.guestlist),
                $"{gathering.Id}:10"));

        public static CardinalNotification CompanionGatheringCreated(UserShard companion, GatheringShard gathering)
            => IssueReminder(new("Companion Gathering",
                $"{companion.Name} just created {gathering.Title}",
                new GatheringDeepLink(gathering.Id)));
    }
}

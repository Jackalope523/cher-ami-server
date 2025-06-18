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

    public struct GroupDeepLink : IDeepLink
    {
        public enum FocusTarget
        {
            guestlist,
            gallery,
        }

        public string RelativePath { get; private set; }

        public GroupDeepLink(long groupId,
            FocusTarget? focus = null, string invitedBy = null)
        {
            string path = $"group/{groupId}";
            
            string options = "";

            options += IDeepLink.ParseOption("focus", focus);
            options += IDeepLink.ParseOption("invited_by", invitedBy);

            RelativePath = IDeepLink.FormatPath(path, options);
        }
    }

    public struct ProfileDeepLink : IDeepLink
    {
        public string RelativePath { get; private set; }

        public ProfileDeepLink(long userId)
        {
            string path = $"profile/{userId}";

            string options = "";

            RelativePath = IDeepLink.FormatPath(path, options);
        }
    }

    public struct MessageDeepLink : IDeepLink
    {
        public string RelativePath { get; private set; }

        public MessageDeepLink(long chatId)
        {
            string path = $"chat/{conversationId}";

            RelativePath = IDeepLink.FormatPath(path);
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

    /////////
    // Social Invitations
    ///////////////////////

    public partial class CardinalNotification
    {
        protected static CardinalNotification SocialInvitation(CardinalNotification notification)
        {
            notification.Group = NotificationGroup.SocialInvitations;
            return notification;
        }

        public static CardinalNotification CompanionshipRequest(UserShard addingUser, string lastMet = null)
            => SocialInvitation(new("Companion Request",
                $"{addingUser.Name} sent you a companionship request.",
                new ProfileDeepLink(addingUser.Id, lastMet),
                "1"));

        public static CardinalNotification CompanionshipForged(UserShard addingUser)
            => SocialInvitation(new("New Companion",
                $"Companionship forged with {addingUser.Name} accepted.",
                new ProfileDeepLink(addingUser.Id),
                "1"));

        public static CardinalNotification GatheringInvitation(UserShard invitingUser, GatheringShard gathering)
            => SocialInvitation(new("Gathering Invitation",
                $"{invitingUser.Name} invited you to {gathering.Title}.",
                new GatheringDeepLink(gathering.Id, invitedBy: invitingUser.Name),
                $"{gathering.Id}:1"));
    }

    /////////
    // Companion Activity
    ///////////////////////

    public partial class CardinalNotification
    {
        protected static CardinalNotification CompanionActivity(CardinalNotification notification)
        {
            notification.Group = NotificationGroup.CompanionActivity;
            return notification;
        }

        public static CardinalNotification CompanionJoined(UserShard companion, GatheringShard gathering)
            => CompanionActivity(new(gathering.Title,
                $"{companion.Name} joined the gathering.",
                new GatheringDeepLink(gathering.Id, focus: GroupDeepLink.FocusTarget.guestlist),
                $"{gathering.Id}:10"));

        public static CardinalNotification CompanionGatheringCreated(UserShard companion, GatheringShard gathering)
            => CompanionActivity(new("Companion Gathering",
                $"{companion.Name} just created {gathering.Title}",
                new GatheringDeepLink(gathering.Id)));
    }

    //////////
    // Gathering Discovery
    ////////////////////////

    public partial class CardinalNotification
    {
        protected static CardinalNotification GatheringDiscovery(CardinalNotification notification)
        {
            notification.Group = NotificationGroup.GatheringDiscovery;
            return notification;
        }

        public static CardinalNotification NearbyGatherings() // TODO Slot in
            => GatheringDiscovery(new("New Gatherings Nearby",
                "There are new gatherings in your area that you may be interested in.",
                new DiscoveryDeepLink()));
        // TODO A. Need to actually ensure that they are new (gathering creation time vs last logged in) B. not send multiple
        // ^Advanced profile and filter system

        public static CardinalNotification CompanionMotive(GatheringShard gathering) // TODO Slot in
            => GatheringDiscovery(new("Companion Movement",
                "Your companions are headed somewhere interesting...",
                new GatheringDeepLink(gathering.Id)));
    }

    //////////
    // Gathering Reminders
    ////////////////////////

    public partial class CardinalNotification
    {
        protected static CardinalNotification GatheringReminders(CardinalNotification notification)
        {
            notification.Group = NotificationGroup.GatheringReminders;
            return notification;
        }

        public static CardinalNotification GatheringUpcoming(GatheringShard gathering, string relativeTime = "later")
            => GatheringReminders(new(gathering.Title,
                $"Is starting {relativeTime}.",
                new GatheringDeepLink(gathering.Id),
                "20"));

        public static CardinalNotification GatheringImminent(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Is starting shortly.",
                new GatheringDeepLink(gathering.Id, immediate: true),
                "20"));

        public static CardinalNotification GatheringLive(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Is now live!",
                new GatheringDeepLink(gathering.Id, immediate: true),
                "20"));

        public static CardinalNotification GatheringCancelled(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Was cancelled by the host.",
                new GatheringDeepLink(gathering.Id),
                "20"));

        public static CardinalNotification GatheringEdited(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Was modified by the host.",
                new GatheringDeepLink(gathering.Id),
                "21"));

        public static CardinalNotification GatheringUploadClosing(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Don't forget to post your remaining photos!",
                new GatheringDeepLink(gathering.Id, focus: GroupDeepLink.FocusTarget.gallery)));
    }

    /////////
    // Gathering Activity
    ///////////////////////

    public partial class CardinalNotification
    {
        protected static CardinalNotification GatheringActivity(CardinalNotification notification)
        {
            notification.Group = NotificationGroup.GatheringActivity;
            return notification;
        }

        // Host

        public static CardinalNotification GatheringSealed(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                $"Was reported too many times and was sealed as a result.",
                new GatheringDeepLink(gathering.Id, @sealed: true)));

        public static CardinalNotification GatheringHeartbeat(GatheringShard gathering) // TODO Slot in
            => GatheringActivity(new(gathering.Title,
                $"Is the gathering still ongoing?",
                new GatheringDeepLink(gathering.Id, immediate: true)));

        public static CardinalNotification HostLeavingGatheringArea(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                $"You are leaving the gathering area, gathering will hide itself.",
                new GatheringDeepLink(gathering.Id)));

        // Attendee

        public static CardinalNotification AttendeeLeavingGatheringArea(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                $"You are leaving the gathering area.",
                new GatheringDeepLink(gathering.Id),
                "30"));

        public static CardinalNotification GatheringTerminated(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                $"Has ended. Thanks for joining!",
                new GatheringDeepLink(gathering.Id),
                "30"));

        public static CardinalNotification UserMissedGathering(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                "You missed the gathering.",
                new GatheringDeepLink(gathering.Id),
                "20"));
    }

    //////
    // Messages
    /////////////

    public partial class CardinalNotification
    {
        protected static CardinalNotification Message(CardinalNotification notification)
        {
            return notification;
        }

        public static CardinalNotification IndividualMessage(ChatShard conversation, UserShard sender, MessageShard message)
            => Message(new(sender.Name,
                ParseMessage(message),
                new MessageDeepLink(conversation.Id),
                $"chat:{conversation.Id}"));

        public static CardinalNotification GroupMessage(GroupShard group, ChatShard conversation, UserShard sender, MessageShard message)
            => Message(new(sender.Name,
                group.Title,
                ParseMessage(message),
                new MessageDeepLink(conversation.Id),
                $"chat:{conversation.Id}"));

        private static string ParseMessage(MessageShard message)
        {
            return message.Type switch
            {
                MessageType.Text => message.Value.ToString(),
                MessageType.Photo => "Sent a photo.",
                MessageType.Issue => "Shared a segment.",
                MessageType.Post => "Shared a post.",
                MessageType.Profile => "Shared a profile.",
                _ => "",
            };
        }
    }
}

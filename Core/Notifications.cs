using System;
using System.Reflection;
using Core.Boundaries;
using Microsoft.VisualBasic;

namespace Core.Notifications
{
    public enum NotificationGroup
    {
        None,
        SocialInvitations,
        CompanionActivity,
        GatheringDiscovery,
        GatheringReminders,
        GatheringActivity,
    }

    public static class NotificationGroupExtensions
    {
        public static bool CheckEnabled(this NotificationGroup group, NotificationProfile profile)
        {
            return group switch
            {
                NotificationGroup.None => true,
                NotificationGroup.SocialInvitations => profile.SocialInvitations,
                NotificationGroup.CompanionActivity => profile.CompanionActivity,
                NotificationGroup.GatheringDiscovery => profile.GatheringDiscovery,
                NotificationGroup.GatheringReminders => profile.GatheringReminders,
                NotificationGroup.GatheringActivity => profile.GatheringActivity,
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

    public struct GatheringDeepLink : IDeepLink
    {
        public enum FocusTarget
        {
            guestlist,
            gallery,
        }

        public string RelativePath { get; private set; }

        public GatheringDeepLink(long gatheringId,
            FocusTarget? focus = null, string invitedBy = null,
            bool? immediate = null, bool? @sealed = null)
        {
            string path = $"gathering/{gatheringId}";
            
            string options = "";

            options += IDeepLink.ParseOption("focus", focus);
            options += IDeepLink.ParseOption("invited_by", invitedBy);
            options += IDeepLink.ParseOption("immediate", immediate);
            options += IDeepLink.ParseOption("sealed", @sealed);

            RelativePath = IDeepLink.FormatPath(path, options);
        }
    }

    public struct DiscoveryDeepLink : IDeepLink
    {
        public string RelativePath => IDeepLink.FormatPath("discovery");
    }

    public struct NestDeepLink : IDeepLink
    {
        public string RelativePath { get; private set; }

        public NestDeepLink(long userId,
            string lastMet = null)
        {
            string path = $"nest/{userId}";

            string options = "";

            options += IDeepLink.ParseOption("last_met", lastMet);

            RelativePath = IDeepLink.FormatPath(path, options);
        }
    }

    public struct MessageDeepLink : IDeepLink
    {
        public string RelativePath { get; private set; }

        public MessageDeepLink(long conversationId)
        {
            string path = $"chat/{conversationId}";

            RelativePath = IDeepLink.FormatPath(path);
        }
    }

    public partial class CanaryNotification
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Body { get; set; }
        public string AppUrl { get; set; }
        public string ThreadId { get; set; }

        public NotificationGroup Group { get; set; }

        protected CanaryNotification(string title, string body, IDeepLink deepLink = null, string threadId = "")
        {
            Title = title;
            Body = body;
            AppUrl = deepLink != null ? deepLink.RelativePath : "";
            ThreadId = threadId;
            Group = NotificationGroup.None;
        }

        protected CanaryNotification(string title, string subtitle, string body, IDeepLink deepLink = null, string threadId = "")
            : this(title, body, deepLink, threadId)
        {
            Subtitle = subtitle;
        }

        public bool CheckEnabled(NotificationProfile profile)
        {
            return Group.CheckEnabled(profile);
        }
    }

    /////////
    // Social Invitations
    ///////////////////////

    public partial class CanaryNotification
    {
        protected static CanaryNotification SocialInvitation(CanaryNotification notification)
        {
            notification.Group = NotificationGroup.SocialInvitations;
            return notification;
        }

        public static CanaryNotification CompanionshipRequest(UserShard addingUser, string lastMet = null)
            => SocialInvitation(new("Companion Request",
                $"{addingUser.Name} sent you a companionship request.",
                new NestDeepLink(addingUser.Id, lastMet),
                "1"));

        public static CanaryNotification CompanionshipForged(UserShard addingUser)
            => SocialInvitation(new("New Companion",
                $"Companionship forged with {addingUser.Name} accepted.",
                new NestDeepLink(addingUser.Id),
                "1"));

        public static CanaryNotification GatheringInvitation(UserShard invitingUser, GatheringShard gathering)
            => SocialInvitation(new("Gathering Invitation",
                $"{invitingUser.Name} invited you to {gathering.Title}.",
                new GatheringDeepLink(gathering.Id, invitedBy: invitingUser.Name),
                $"{gathering.Id}:1"));
    }

    /////////
    // Companion Activity
    ///////////////////////

    public partial class CanaryNotification
    {
        protected static CanaryNotification CompanionActivity(CanaryNotification notification)
        {
            notification.Group = NotificationGroup.CompanionActivity;
            return notification;
        }

        public static CanaryNotification CompanionJoined(UserShard companion, GatheringShard gathering)
            => CompanionActivity(new(gathering.Title,
                $"{companion.Name} joined the gathering.",
                new GatheringDeepLink(gathering.Id, focus: GatheringDeepLink.FocusTarget.guestlist),
                $"{gathering.Id}:10"));

        public static CanaryNotification CompanionGatheringCreated(UserShard companion, GatheringShard gathering)
            => CompanionActivity(new("Companion Gathering",
                $"{companion.Name} just created {gathering.Title}",
                new GatheringDeepLink(gathering.Id)));
    }

    //////////
    // Gathering Discovery
    ////////////////////////

    public partial class CanaryNotification
    {
        protected static CanaryNotification GatheringDiscovery(CanaryNotification notification)
        {
            notification.Group = NotificationGroup.GatheringDiscovery;
            return notification;
        }

        public static CanaryNotification NearbyGatherings() // TODO Slot in
            => GatheringDiscovery(new("New Gatherings Nearby",
                "There are new gatherings in your area that you may be interested in.",
                new DiscoveryDeepLink()));
        // TODO A. Need to actually ensure that they are new (gathering creation time vs last logged in) B. not send multiple
        // ^Advanced profile and filter system

        public static CanaryNotification CompanionMotive(GatheringShard gathering) // TODO Slot in
            => GatheringDiscovery(new("Companion Movement",
                "Your companions are headed somewhere interesting...",
                new GatheringDeepLink(gathering.Id)));
    }

    //////////
    // Gathering Reminders
    ////////////////////////

    public partial class CanaryNotification
    {
        protected static CanaryNotification GatheringReminders(CanaryNotification notification)
        {
            notification.Group = NotificationGroup.GatheringReminders;
            return notification;
        }

        public static CanaryNotification GatheringUpcoming(GatheringShard gathering, string relativeTime = "later")
            => GatheringReminders(new(gathering.Title,
                $"Is starting {relativeTime}.",
                new GatheringDeepLink(gathering.Id),
                "20"));

        public static CanaryNotification GatheringImminent(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Is starting shortly.",
                new GatheringDeepLink(gathering.Id, immediate: true),
                "20"));

        public static CanaryNotification GatheringLive(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Is now live!",
                new GatheringDeepLink(gathering.Id, immediate: true),
                "20"));

        public static CanaryNotification GatheringCancelled(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Was cancelled by the host.",
                new GatheringDeepLink(gathering.Id),
                "20"));

        public static CanaryNotification GatheringEdited(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Was modified by the host.",
                new GatheringDeepLink(gathering.Id),
                "21"));

        public static CanaryNotification GatheringUploadClosing(GatheringShard gathering)
            => GatheringReminders(new(gathering.Title,
                $"Don't forget to post your remaining photos!",
                new GatheringDeepLink(gathering.Id, focus: GatheringDeepLink.FocusTarget.gallery)));
    }

    /////////
    // Gathering Activity
    ///////////////////////

    public partial class CanaryNotification
    {
        protected static CanaryNotification GatheringActivity(CanaryNotification notification)
        {
            notification.Group = NotificationGroup.GatheringActivity;
            return notification;
        }

        // Host

        public static CanaryNotification GatheringSealed(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                $"Was reported too many times and was sealed as a result.",
                new GatheringDeepLink(gathering.Id, @sealed: true)));

        public static CanaryNotification GatheringHeartbeat(GatheringShard gathering) // TODO Slot in
            => GatheringActivity(new(gathering.Title,
                $"Is the gathering still ongoing?",
                new GatheringDeepLink(gathering.Id, immediate: true)));

        public static CanaryNotification HostLeavingGatheringArea(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                $"You are leaving the gathering area, gathering will hide itself.",
                new GatheringDeepLink(gathering.Id)));

        // Attendee

        public static CanaryNotification AttendeeLeavingGatheringArea(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                $"You are leaving the gathering area.",
                new GatheringDeepLink(gathering.Id),
                "30"));

        public static CanaryNotification GatheringTerminated(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                $"Has ended. Thanks for joining!",
                new GatheringDeepLink(gathering.Id),
                "30"));

        public static CanaryNotification UserMissedGathering(GatheringShard gathering)
            => GatheringActivity(new(gathering.Title,
                "You missed the gathering.",
                new GatheringDeepLink(gathering.Id),
                "20"));
    }

    //////
    // Messages
    /////////////

    public partial class CanaryNotification
    {
        protected static CanaryNotification Message(CanaryNotification notification)
        {
            return notification;
        }

        public static CanaryNotification IndividualMessage(ConversationShard conversation, UserShard sender, MessageShard message)
            => Message(new(sender.Name,
                ParseMessage(message),
                new MessageDeepLink(conversation.Id),
                $"chat:{conversation.Id}"));

        public static CanaryNotification GroupMessage(ConversationShard conversation, UserShard sender, MessageShard message)
            => Message(new(sender.Name,
                conversation.Title,
                ParseMessage(message),
                new MessageDeepLink(conversation.Id),
                $"chat:{conversation.Id}"));

        public static CanaryNotification GatheringMessage(GatheringShard gathering, ConversationShard conversation, UserShard sender, MessageShard message)
            => Message(new(sender.Name,
                gathering.Title,
                ParseMessage(message),
                new MessageDeepLink(conversation.Id),
                $"chat:{conversation.Id}"));

        private static string ParseMessage(MessageShard message)
        {
            return message.Type switch
            {
                MessageType.Text => message.Value.ToString(),
                MessageType.Photo => "Sent a photo.",
                MessageType.GatheringInvite => "Invited you to a gathering.",
                MessageType.ShareGathering => "Shared a gathering.",
                MessageType.Snapshot => "Shared a snapshot.",
                MessageType.Nest => "Shared a nest.",
                _ => "",
            };
        }
    }
}

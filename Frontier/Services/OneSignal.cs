using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Notifications;
using OneSignalApi.Api;
using OneSignalApi.Client;
using OneSignalApi.Model;

namespace Frontier.Services
{
    public class OneSignalService : INotificationService
    {
        private static ILogger log;
        private static DefaultApi instance;

        private static string appId;

        public static void Initialise(ILogger logger, string apiAccessToken, string apiAppId)
        {
            log = logger;

            appId = apiAppId;

            var appConfig = new Configuration
            {
                BasePath = "https://onesignal.com/api/v1",
                AccessToken = apiAccessToken
            };

            instance = new DefaultApi(appConfig);
        }

        public async Task<string> DispatchNotification(CanaryNotification notification, params NotificationProfile[] notificationProfiles)
        {
            List<string> outgoingIds = RetrieveValidTargets(notification, notificationProfiles);

            // Short-circuit if no valid targets
            if (outgoingIds.Count == 0)
            {
                return "";
            }

            var notif = new Notification(appId: appId)
            {
                Headings = new StringMap(en: notification.Title),
                Subtitle = new StringMap(en: notification.Subtitle),
                Contents = new StringMap(en: notification.Body),
                ChannelForExternalUserIds = "push",
                IncludeExternalUserIds = outgoingIds, // Deprecated is a mistake, leave as is

                AppUrl = notification.AppUrl,
                ThreadId = notification.ThreadId,
            };


            string returnedId = "";

            try
            {
                var ret = await instance.CreateNotificationAsync(notif);
                returnedId = ret.Id;
            }
            catch (Exception e)
            {
                log.LogError("Error creating notification {e}", e);
            }

            return returnedId;
        }

        public async Task<string> ScheduleNotification(CanaryNotification notification, DateTimeOffset dispatchAt, params NotificationProfile[] notificationProfiles)
        {
            List<string> outgoingIds = RetrieveValidTargets(notification, notificationProfiles);

            // Short-circuit if no valid targets
            if (outgoingIds.Count == 0)
            {
                return "";
            }

            var notif = new Notification(appId: appId)
            {
                Headings = new StringMap(en: notification.Title),
                Subtitle = new StringMap(en: notification.Subtitle),
                Contents = new StringMap(en: notification.Body),
                ChannelForExternalUserIds = "push",
                IncludeExternalUserIds = outgoingIds, // Deprecated is a mistake, leave as is

                AppUrl = notification.AppUrl,
                CollapseId = notification.ThreadId,
                SendAfter = dispatchAt.DateTime,
            };

            string returnedId = "";

            try
            {
                var ret = await instance.CreateNotificationAsync(notif);
                returnedId = ret.Id;
            }
            catch (Exception e)
            {
                log.LogError("Error creating notification {e}", e);
            }

            return returnedId;
        }

        public async Task CancelNotification(string notificationId)
        {
            if (string.IsNullOrEmpty(notificationId))
            { return; }

            await instance.CancelNotificationAsync(appId, notificationId);
        }

        public List<string> RetrieveValidTargets(CanaryNotification notification, params NotificationProfile[] notificationProfiles)
        {
            List<string> targets = new();

            foreach (var profile in notificationProfiles)
            {
                // Check valid target
                if (profile.NotificationId.Equals(Guid.Empty))
                {
                    log.LogWarning("Tried to push notification to empty user.\nTitle {title}\nBody {body}", notification.Title, notification.Body);
                    continue;
                }

                // Check user preferences
                if (notification.CheckEnabled(profile))
                {
                    targets.Add(profile.NotificationId.ToString());
                }
            }

            return targets;
        }
    }
}

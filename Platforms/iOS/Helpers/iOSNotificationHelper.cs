using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserNotifications;

namespace Cardrly.Platforms.iOS.Helpers
{
    public static class iOSNotificationHelper
    {
        public static void SendOnce(string id, string title, string body)
        {
            var content = new UNMutableNotificationContent
            {
                Title = title,
                Body = body,
                Sound = UNNotificationSound.Default
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);

            var request = UNNotificationRequest.FromIdentifier(id, content, trigger);
            UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
        }

        public static void Cancel(string id)
        {
            UNUserNotificationCenter.Current.RemovePendingNotificationRequests(new[] { id });
            UNUserNotificationCenter.Current.RemoveDeliveredNotifications(new[] { id });
        }
    }
}

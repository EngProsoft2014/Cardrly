using CoreFoundation;
using Foundation;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UserNotifications;

namespace Cardrly.Platforms.iOS.Helpers
{
    public static class NetworkHelper
    {

        private static NWPathMonitor _monitor;
        private static NWPathStatus _lastStatus = NWPathStatus.Unsatisfied;

        public static void StartMonitoring()
        {
            if (_monitor != null) return;

            _monitor = new NWPathMonitor();

            // Assign handler for path updates
            _monitor.SnapshotHandler = (path) =>
            {
                _lastStatus = path.Status;

                if (_lastStatus == NWPathStatus.Unsatisfied)
                {
                    iOSNotificationHelper.SendOnce(
                        "InternetUnavailable",
                        "Internet Unavailable",
                        "Location will be sent when internet is restored."
                    );
                }
                else
                {
                    iOSNotificationHelper.Cancel("InternetUnavailable");
                }
            };

            // You must set a queue before Start()
            var queue = new DispatchQueue("NetworkMonitor");
            _monitor.SetQueue(queue);

            _monitor.Start();

            // 🔔 Schedule reminder notification when tracking stops
            ScheduleReminderNotification();
        }

        public static void StopMonitoring()
        {
            if (_monitor != null)
            {
                try
                {
                    _monitor.Cancel();
                    _monitor.Dispose();
                }
                catch (Exception)
                {

                }

                _monitor = null;
                _lastStatus = NWPathStatus.Unsatisfied;

                CancelReminderNotification();
            }
        }

        public static bool IsInternetAvailable()
        {
            return _lastStatus == NWPathStatus.Satisfied;
        }


        // 🔔 Schedule reminder notification
        private static void ScheduleReminderNotification()
        {
            var center = UNUserNotificationCenter.Current;

            var content = new UNMutableNotificationContent
            {
                Title = "Internet Unavailable",
                Body = "Location will be sent when internet is restored.",
                Sound = UNNotificationSound.Default
            };

            // Trigger after 3 min, repeat
            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(180, false); // 3 min

            var request = UNNotificationRequest.FromIdentifier("InternetReminder", content, trigger);

            center.AddNotificationRequest(request, (err) =>
            {
                if (err != null)
                    Console.WriteLine($"Error scheduling notification: {err}");
            });
        }

        // ❌ Cancel reminder notification
        private static void CancelReminderNotification()
        {
            var center = UNUserNotificationCenter.Current;
            center.RemovePendingNotificationRequests(new[] { "InternetReminder" });
            center.RemoveDeliveredNotifications(new[] { "InternetReminder" });
        }
    }
}

using Foundation;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Platforms.iOS.Helpers
{
    public static class NetworkHelper
    {
        private static bool _initialized;

        public static bool IsInternetAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public static void StartMonitoring()
        {
            if (_initialized) return;

            // Subscribe to network availability changes
            NetworkChange.NetworkAvailabilityChanged += (sender, e) =>
            {
                if (!e.IsAvailable)
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

            _initialized = true;
        }
        public static void StopMonitoring()
        {
            if (_initialized)
            {
                // Unsubscribe to avoid leaks
                NetworkChange.NetworkAvailabilityChanged -= (sender, e) => { };
                _initialized = false;
            }
        }

    }
}

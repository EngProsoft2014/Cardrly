using CoreFoundation;
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
        }

        public static void StopMonitoring()
        {
            if (_monitor != null)
            {
                _monitor.Cancel();
                _monitor.Dispose();
                _monitor = null;
            }
        }

        public static bool IsInternetAvailable()
        {
            return _lastStatus == NWPathStatus.Satisfied;
        }

    }
}

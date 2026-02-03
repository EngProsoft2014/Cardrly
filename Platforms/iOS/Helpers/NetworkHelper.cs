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
        //public static bool IsInternetAvailable()
        //{
        //    return NetworkInterface.GetIsNetworkAvailable();
        //}

        private static NWPathMonitor _monitor;
        private static bool _initialized;
        private static NWPathStatus _lastStatus = NWPathStatus.Unsatisfied;

        public static void StartMonitoring()
        {
            if (_initialized) return;

            _monitor = new NWPathMonitor();
            _monitor.Start();

            _initialized = true;
        }

        public static void StopMonitoring()
        {
            if (_monitor != null)
            {
                _monitor.Cancel();
                _monitor.Dispose();
                _monitor = null;
                _initialized = false;
            }
        }

        public static bool IsInternetAvailable()
        {
            if (_monitor == null) return false;
            return _monitor.CurrentPath?.Status == NWPathStatus.Satisfied;
        }

    }
}

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
        public static bool IsInternetAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
    }
}


using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Platforms.Android.Services
{
    public static class ServiceHelper
    {
        public static bool IsServiceRunning(Context context, System.Type serviceType)
        {
            var manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
            foreach (var service in manager.GetRunningServices(int.MaxValue))
            {
                if (service.Service.ClassName.Equals(serviceType.FullName))
                    return true;
            }
            return false;
        }
    }
}


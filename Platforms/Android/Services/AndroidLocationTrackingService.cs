
using Android.Content;
using Cardrly.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Android.App.Application;


namespace Cardrly.Platforms.Android.Services
{
    public class AndroidLocationTrackingService : IPlatformLocationService
    {
        public void StartBackgroundTracking(string employeeId)
        {
            var context = Application.Context;
            var serviceType = typeof(LocationService);

            // ✅ Only start if not already running
            if (!ServiceHelper.IsServiceRunning(context, serviceType))
            {
                var intent = new Intent(context, serviceType);
                intent.PutExtra("EmployeeId", employeeId);
                context.StartForegroundService(intent);
            }
        }

        public void StopBackgroundTracking()
        {
            var context = Application.Context;
            var intent = new Intent(context, typeof(LocationService));

            // 🔥 Stop reminder loop when service is stopped
            LocationService.StopReminderLoop();

            context.StopService(intent);
        }
    }
}

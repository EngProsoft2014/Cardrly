
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
        public void StartBackgroundTracking(string employeeId, EventHandler<GeolocationLocationChangedEventArgs> callback)
        {
            var context = Application.Context; // avoids ambiguity with MAUI Application
            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(Cardrly.Platforms.Android.Services.LocationService)));
            intent.PutExtra("EmployeeId", employeeId);
            context.StartForegroundService(intent);
        }

        public void StopBackgroundTracking()
        {
            var context = Application.Context;
            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(Cardrly.Platforms.Android.Services.LocationService)));
            context.StopService(intent);
        }
    }
}

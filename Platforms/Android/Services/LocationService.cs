using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.Core.App;
using Cardrly.Constants;
using Cardrly.Models;
using Cardrly.Services.Data;
using CommunityToolkit.Mvvm.Messaging;


namespace Cardrly.Platforms.Android.Services
{
    [Service(ForegroundServiceType = ForegroundService.TypeLocation, Exported = false)]
    public class LocationService : Service, ILocationListener
    {
        private LocationManager _locationManager;
        private string _employeeId;
        private SignalRService _signalR;

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info("LocationService", "OnCreate fired");

            _signalR = MauiApplication.Current.Services.GetRequiredService<SignalRService>();
            _locationManager = (LocationManager)GetSystemService(LocationService);

            // Create notification channel (required on Android 8+)
            var channelId = "location_channel";
            var channel = new NotificationChannel(channelId, "Location Tracking", NotificationImportance.High);
            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);

            // Build persistent notification
            var notification = new NotificationCompat.Builder(this, channelId)
                .SetContentTitle("Tracking Location")
                .SetContentText("Your location is being tracked")
                .SetSmallIcon(Resource.Drawable.gps)
                .SetOngoing(true) // makes it persistent
                .Build();

            // Promote service to foreground
            StartForeground(1, notification);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Info("LocationService", "OnStartCommand fired");

            _employeeId = intent.GetStringExtra("EmployeeId") ?? string.Empty;

            // Request GPS updates every 5 seconds
            _locationManager.RequestLocationUpdates(
                LocationManager.GpsProvider,
                5000, // milliseconds
                0,    // meters
                this);

            return StartCommandResult.Sticky;
        }

        // Called when a new location is available
        public void OnLocationChanged(global::Android.Locations.Location location)
        {
            Log.Info("LocationService", "OnLocationChanged fired");

            var data = new DataMapsModel
            {
                EmployeeId = _employeeId,
                AccountId = Preferences.Default.Get(ApiConstants.AccountId, string.Empty),
                TimeSheetId = Preferences.Default.Get(ApiConstants.TimeSheetId, string.Empty),
                Lat = location.Latitude,
                Long = location.Longitude,
                CreateDate = DateTime.UtcNow,
                Time = DateTime.UtcNow.TimeOfDay
            };

            _signalR.SendEmployeeLocation(data);
        }

        // Called when GPS provider is disabled
        public void OnProviderDisabled(string provider)
        {
            Log.Warn("LocationService", $"Provider disabled: {provider}");

            var notification = new NotificationCompat.Builder(this, "location_channel")
                .SetContentTitle("Location Tracking Paused")
                .SetContentText("Please enable GPS to continue tracking")
                .SetSmallIcon(Resource.Drawable.gps)
                .Build();

            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.Notify(2, notification);

            WeakReferenceMessenger.Default.Send(new GpsStatusMessage(false));
        }

        // Called when GPS provider is enabled
        public void OnProviderEnabled(string provider)
        {
            Log.Info("LocationService", $"Provider enabled: {provider}");
            // Optionally cancel the "paused" notification
            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.Cancel(2);

            WeakReferenceMessenger.Default.Send(new GpsStatusMessage(true));
        }

        // Called when provider status changes
        public void OnStatusChanged(string? provider, [GeneratedEnum] Availability status, Bundle? extras)
        {
            Log.Debug("LocationService", $"Provider {provider} status changed: {status}");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _locationManager?.RemoveUpdates(this);
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            Log.Info("LocationService", "Task removed, stopping service");
            StopSelf(); // stops the service → notification disappears
        }


        public override IBinder OnBind(Intent intent) => null!;
    }
}

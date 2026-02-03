using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Net;
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
        private bool _isListening;
        private static CancellationTokenSource _notifyCts;
        private static volatile bool _internetReminderRunning;

        private ConnectivityManager _connectivityManager;
        private ConnectivityManager.NetworkCallback _networkCallback;

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info("LocationService", "OnCreate fired");

            _connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);

            _networkCallback = new NetworkCallbackImpl(this);
            _connectivityManager.RegisterDefaultNetworkCallback(_networkCallback);

            _signalR = MauiApplication.Current.Services.GetRequiredService<SignalRService>();
            _locationManager = (LocationManager)GetSystemService(LocationService);

            // Create notification channel (required on Android 8+)
            var channelId = "location_channel";
            var channel = new NotificationChannel(channelId, "Location Tracking", NotificationImportance.High);
            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);

            // 🔥 Clear all old notifications when service starts
            manager.CancelAll();

            // 🔥 Stop any old reminder loop when service starts
            StopReminderLoop();

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

            if (_isListening)
                return StartCommandResult.Sticky; // already tracking

            // Request GPS updates every 5 seconds
            _locationManager.RequestLocationUpdates(
                LocationManager.GpsProvider,
                5000, // milliseconds
                10,    // meters
                this);

            _isListening = true;

            return StartCommandResult.Sticky;
        }

        // Called when a new location is available
        public void OnLocationChanged(global::Android.Locations.Location location)
        {
            Log.Info("LocationService", "OnLocationChanged fired");

            if (!IsInternetAvailable())
            {
                if (!_internetReminderRunning)
                    StartInternetReminderLoop();

                return;
            }

            StopReminderLoop();

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

            try
            {
                _signalR.SendEmployeeLocation(data);
            }
            catch
            {
                if (!IsInternetAvailable() && !_internetReminderRunning)
                    StartInternetReminderLoop();
            }

        }

        // Called when GPS provider is disabled
        public void OnProviderDisabled(string provider)
        {
            Log.Warn("LocationService", $"Provider disabled: {provider}");

            // Cancel any existing loop
            StopReminderLoop();

            // Create a new CTS
            _notifyCts = new CancellationTokenSource();
            var token = _notifyCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var notification = new NotificationCompat.Builder(this, "location_channel")
                        .SetContentTitle("Location Sharing Stopped")
                        .SetContentText("Please reopen the app, enable GPS, or allow location permission to resume sending your location.")
                        .SetSmallIcon(Resource.Drawable.gps)
                        .SetAutoCancel(true)
                        .Build();

                    var manager = (NotificationManager)GetSystemService(NotificationService);
                    manager.Notify(2, notification);

                    await Task.Delay(TimeSpan.FromMinutes(3), token);
                }
            }, token);


            WeakReferenceMessenger.Default.Send(new GpsStatusMessage(false));
        }

        // Called when GPS provider is enabled
        public void OnProviderEnabled(string provider)
        {
            Log.Info("LocationService", $"Provider enabled: {provider}");

            // Stop the repeating notifications
            StopReminderLoop();

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
            if (_isListening)
            {
                _locationManager?.RemoveUpdates(this);
                _isListening = false;
            }

            if (_connectivityManager != null && _networkCallback != null)
            {
                _connectivityManager.UnregisterNetworkCallback(_networkCallback);
            }
        }


        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            Log.Info("LocationService", "Task removed, stopping service");

            // Stop the service → foreground notification disappears
            StopSelf();

            // Cancel any previous reminder loop
            StopReminderLoop();

            // Create a new CTS
            _notifyCts = new CancellationTokenSource();
            var token = _notifyCts.Token;

            // Start reminder notifications every 1 minute until app is reopened
            var manager = (NotificationManager)GetSystemService(NotificationService);

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var notification = new NotificationCompat.Builder(this, "location_channel")
                        .SetContentTitle("Location Sharing Stopped")
                        .SetContentText("Please reopen the app, enable GPS, or allow location permission to resume sending your location.")
                        .SetSmallIcon(Resource.Drawable.gps)
                        .SetAutoCancel(true)
                        .Build();

                    manager.Notify(3, notification);

                    await Task.Delay(TimeSpan.FromMinutes(3), token);
                }
            }, token);
        }

        public static void StopReminderLoop()
        {
            if (_notifyCts != null)
            {
                _notifyCts.Cancel();   // cancels the loop
                _notifyCts.Dispose();  // free resources
                _notifyCts = null;
            }
        }


        public static void ClearReminders(Context context)
        {
            // Stop loop
            StopReminderLoop();

            // Cancel notifications
            var manager = (NotificationManager)context.GetSystemService(NotificationService);
            manager.Cancel(2); // GPS disabled
            manager.Cancel(3); // App removed reminder
            manager.Cancel(4); // Internet disconnected
        }

        private bool IsInternetAvailable()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            var network = cm.ActiveNetwork;
            if (network == null) return false;

            var caps = cm.GetNetworkCapabilities(network);
            return caps != null && caps.HasCapability(NetCapability.Internet);
        }

        private void StartInternetReminderLoop()
        {
            if (_internetReminderRunning)
                return;

            _internetReminderRunning = true;

            StopReminderLoop();

            _notifyCts = new CancellationTokenSource();
            var token = _notifyCts.Token;

            Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (!IsInternetAvailable())
                        {
                            var notification = new NotificationCompat.Builder(this, "location_channel")
                                .SetContentTitle("Internet Disconnected")
                                .SetContentText("Location will be sent when internet is restored.")
                                .SetSmallIcon(Resource.Drawable.gps)
                                .SetAutoCancel(true)
                                .Build();

                            var manager = (NotificationManager)GetSystemService(NotificationService);
                            manager.Notify(4, notification);
                        }
                        else
                        {
                            break;
                        }

                        await Task.Delay(TimeSpan.FromMinutes(3), token);
                    }
                }
                finally
                {
                    _internetReminderRunning = false;
                    ClearReminders(this);
                }
            }, token);
        }


        public override IBinder OnBind(Intent intent) => null!;


        //==================================================================


        class NetworkCallbackImpl : ConnectivityManager.NetworkCallback
        {
            private readonly LocationService _service;

            public NetworkCallbackImpl(LocationService service)
            {
                _service = service;
            }

            public override void OnLost(Network network)
            {
                base.OnLost(network);

                Log.Warn("LocationService", "Internet lost");

                if (!_internetReminderRunning)
                    _service.StartInternetReminderLoop();
            }

            public override void OnAvailable(Network network)
            {
                base.OnAvailable(network);

                Log.Info("LocationService", "Internet restored");

                ClearReminders(_service);
            }
        }

    }
}

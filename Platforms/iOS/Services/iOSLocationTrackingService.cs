using Cardrly.Models;
using Cardrly.Platforms.iOS.Helpers;
using Cardrly.Services;
using Cardrly.Services.Data;
using CoreBluetooth;
using CoreFoundation;
using CoreLocation;
using Foundation;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;

namespace Cardrly.Platforms.iOS.Services
{
    [Preserve(AllMembers = true)]
    public class iOSLocationTrackingService : IPlatformLocationService
    {
        private readonly SignalRService _signalR;
        private string _employeeId;
        private CLLocationManager _manager;
        private bool _isListening;
        private CLLocation _lastSentLocation;
        private NWPathMonitor _networkMonitor;

        // Movement threshold in meters
        private const double MovementThreshold = 10;

        public iOSLocationTrackingService(SignalRService signalR)
        {
            _signalR = signalR;
        }

        public void StartBackgroundTracking(string employeeId)
        {
            if (_isListening)
                return; // already running

            // 🔥 prevent duplicate managers
            StopBackgroundTracking();

            _employeeId = employeeId;

            _manager = new CLLocationManager
            {
                AllowsBackgroundLocationUpdates = true,
                PausesLocationUpdatesAutomatically = false,
                ShowsBackgroundLocationIndicator = true,
                ActivityType = CLActivityType.OtherNavigation,
                DesiredAccuracy = CLLocation.AccuracyBest,
                DistanceFilter = 1 // receive all updates, we filter manually
            };

            _manager.Delegate = new LocationDelegate(_signalR, _employeeId, this);

            if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
            {
                _manager.RequestTemporaryFullAccuracyAuthorization("Tracking");
            }

            if (CLLocationManager.Status == CLAuthorizationStatus.NotDetermined)
            {
                _manager.RequestAlwaysAuthorization();
            }

            // GPS check before starting
            if (!CLLocationManager.LocationServicesEnabled)
            {
                iOSNotificationHelper.SendOnce(
                    "GpsDisabled",
                    "GPS Disabled",
                    "Please enable location services to continue tracking."
                );
            }

            // Start all location services
            _manager.StartUpdatingLocation();
            _manager.StartMonitoringSignificantLocationChanges();
            _manager.StartMonitoringVisits();

            _isListening = true;

            // ✅ Cancel any reminder notifications if tracking is active
            CancelReminderNotification();

            // Start monitoring internet connectivity
            NetworkHelper.StartMonitoring();
        }

        public void StopBackgroundTracking()
        {
            // Cancel reminder notifications when stopping
            CancelReminderNotification();

            if (_manager != null)
            {
                _manager.StopUpdatingLocation();
                _manager.StopMonitoringSignificantLocationChanges();
                _manager.StopMonitoringVisits();

                _manager.AllowsBackgroundLocationUpdates = false;
                _manager.PausesLocationUpdatesAutomatically = true;

                _manager.Delegate = null;
                _manager.Dispose();
                _manager = null;
            }

            _lastSentLocation = null;
            _isListening = false;

            // Stop network monitoring
            NetworkHelper.StopMonitoring();

            // 🔔 Schedule reminder notification when tracking stops
            ScheduleReminderNotification();
        }

        // Called from delegate to check distance threshold
        internal bool ShouldSendLocation(CLLocation newLocation)
        {
            if (_lastSentLocation == null)
            {
                _lastSentLocation = newLocation;
                return true;
            }

            var distance = newLocation.DistanceFrom(_lastSentLocation); // in meters
            if (distance >= MovementThreshold)
            {
                _lastSentLocation = newLocation;
                return true;
            }

            return false;
        }

        // 🔔 Schedule reminder notification
        private void ScheduleReminderNotification()
        {
            var center = UNUserNotificationCenter.Current;

            var content = new UNMutableNotificationContent
            {
                Title = "Location Sharing Stopped",
                Body = "Please reopen the app, enable GPS, or allow location permission to resume sending your location.",
                Sound = UNNotificationSound.Default
            };

            // Trigger after 3 min, repeat
            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(180, false); // 3 min

            var request = UNNotificationRequest.FromIdentifier("LocationReminder", content, trigger);

            center.AddNotificationRequest(request, (err) =>
            {
                if (err != null)
                    Console.WriteLine($"Error scheduling notification: {err}");
            });
        }

        // ❌ Cancel reminder notification
        private void CancelReminderNotification()
        {
            var center = UNUserNotificationCenter.Current;
            center.RemovePendingNotificationRequests(new[] { "LocationReminder" });
            center.RemoveDeliveredNotifications(new[] { "LocationReminder" });
        }

    }
}



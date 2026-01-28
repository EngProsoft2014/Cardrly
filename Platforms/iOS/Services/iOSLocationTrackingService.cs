using Cardrly.Models;
using Cardrly.Services;
using Cardrly.Services.Data;
using CoreBluetooth;
using CoreLocation;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Cardrly.Platforms.iOS.Services
{
    [Preserve(AllMembers = true)]
    public class iOSLocationTrackingService : IPlatformLocationService
    {
        private readonly SignalRService _signalR;
        private string _employeeId;
        private CLLocationManager _manager;

        // Movement threshold in meters
        private const double MovementThreshold = 10;

        private CLLocation _lastSentLocation;

        public iOSLocationTrackingService(SignalRService signalR)
        {
            _signalR = signalR;
        }

        public void StartBackgroundTracking(string employeeId)
        {
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

            // Start all location services
            _manager.StartUpdatingLocation();
            _manager.StartMonitoringSignificantLocationChanges();
            _manager.StartMonitoringVisits();
        }

        public void StopBackgroundTracking()
        {
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
    }
}



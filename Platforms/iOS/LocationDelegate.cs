using Cardrly.Constants;
using Cardrly.Models;
using Cardrly.Platforms.iOS.Services;
using Cardrly.Services.Data;
using CommunityToolkit.Mvvm.DependencyInjection;
using CoreLocation;
using System;

namespace Cardrly.Platforms.iOS
{
    public class LocationDelegate : CLLocationManagerDelegate
    {
        private readonly SignalRService _signalR;
        private readonly string _employeeId;
        private readonly iOSLocationTrackingService _service;

        public LocationDelegate(SignalRService signalR, string employeeId, iOSLocationTrackingService service)
        {
            _signalR = signalR;
            _employeeId = employeeId;
            _service = service;
        }

        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
        {
            var loc = locations.LastOrDefault();
            if (loc == null) return;

            // Check movement threshold
            if (!_service.ShouldSendLocation(loc))
                return;

            // Allow deferred updates for battery optimization
            manager.AllowDeferredLocationUpdatesUntil(CLLocationDistance.MaxDistance, double.MaxValue);

            SendLocation(loc);
        }

        public override void DidVisit(CLLocationManager manager, CLVisit visit)
        {
            // Handle stationary locations (arrival / departure)
            var loc = new CLLocation(visit.Coordinate.Latitude, visit.Coordinate.Longitude);
            SendLocation(loc);
        }

        private void SendLocation(CLLocation loc)
        {
            var data = new DataMapsModel
            {
                EmployeeId = _employeeId,
                AccountId = Preferences.Default.Get(ApiConstants.AccountId, string.Empty),
                TimeSheetId = Preferences.Default.Get(ApiConstants.TimeSheetId, string.Empty),
                Lat = loc.Coordinate.Latitude,
                Long = loc.Coordinate.Longitude,
                CreateDate = DateTime.UtcNow,
                Time = DateTime.UtcNow.TimeOfDay
            };

            // Send directly to SignalR
            _signalR.SendEmployeeLocation(data);
        }

        public override void DidChangeAuthorization(CLLocationManager manager)
        {
            Console.WriteLine($"Authorization: {manager.AuthorizationStatus}");
        }

        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            Console.WriteLine($"AuthorizationChanged: {status}");
        }
    }
}


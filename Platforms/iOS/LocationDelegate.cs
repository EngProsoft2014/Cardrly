using Cardrly.Constants;
using Cardrly.Models;
using Cardrly.Platforms.iOS.Helpers;
using Cardrly.Platforms.iOS.Services;
using Cardrly.Services.Data;
using CommunityToolkit.Mvvm.DependencyInjection;
using CoreLocation;
using Foundation;
using System;

namespace Cardrly.Platforms.iOS
{
    public class LocationDelegate : CLLocationManagerDelegate
    {
        private readonly SignalRService _signalR;
        private readonly string _employeeId;
        private readonly iOSLocationTrackingService _service;

        private const string InternetNotifId = "InternetUnavailable";
        private const string GpsNotifId = "GpsDisabled";

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

            // Internet check (event-based)
            if (!NetworkHelper.IsInternetAvailable())
            {
                iOSNotificationHelper.SendOnce(
                    InternetNotifId,
                    "Internet Unavailable",
                    "Location will be sent when internet is restored."
                );
                return;
            }

            // ✅ Internet OK → clear warning
            iOSNotificationHelper.Cancel(InternetNotifId);

            SendLocation(loc);
        }


        public override void Failed(CLLocationManager manager, NSError error)
        {
            iOSNotificationHelper.SendOnce(
                GpsNotifId,
                "Location Error",
                "Location services are unavailable. Please enable GPS."
            );
        }

        public override void DidVisit(CLLocationManager manager, CLVisit visit)
        {
            // Handle stationary locations (arrival / departure)
            var loc = new CLLocation(visit.Coordinate.Latitude, visit.Coordinate.Longitude);

            LocationsUpdated(manager, new[] { loc });
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
            try
            {
                _signalR.SendEmployeeLocation(data);
            }
            catch
            {
                // network lost during send
                iOSNotificationHelper.SendOnce(
                    InternetNotifId,
                    "Internet Unavailable",
                    "Location will be sent when internet is restored."
                );
            }
        }

        // 🔹 Permission changes
        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            if (status != CLAuthorizationStatus.AuthorizedAlways)
            {
                iOSNotificationHelper.SendOnce(
                    GpsNotifId,
                    "Location Disabled",
                    "Please enable location permission to continue tracking."
                );
            }
            else
            {
                iOSNotificationHelper.Cancel(GpsNotifId);
            }
        }

    }
}


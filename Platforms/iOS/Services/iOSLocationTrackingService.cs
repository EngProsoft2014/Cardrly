using Cardrly.Models;
using Cardrly.Services;
using Cardrly.Services.Data;
using CoreLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Cardrly.Platforms.iOS.Services
{
    [Foundation.Preserve(AllMembers = true)]
    public class iOSLocationTrackingService : IPlatformLocationService
    {
        private readonly SignalRService _signalR;
        private string _employeeId;
        private CLLocationManager _manager;

        public iOSLocationTrackingService(SignalRService signalR)
        {
            _signalR = signalR;
        }

        public void StartBackgroundTracking(string employeeId, EventHandler<GeolocationLocationChangedEventArgs> callback)
        {
            _employeeId = employeeId;

            _manager = new CLLocationManager
            {
                AllowsBackgroundLocationUpdates = true,
                PausesLocationUpdatesAutomatically = false
            };

            _manager.RequestWhenInUseAuthorization();
            _manager.RequestAlwaysAuthorization();
            _manager.DesiredAccuracy = CLLocation.AccuracyBest;
            _manager.DistanceFilter = 10;

            _manager.LocationsUpdated += (s, e) =>
            {
                var loc = e.Locations.LastOrDefault();
                if (loc != null)
                {
                    // Build your data model
                    var data = new DataMapsModel
                    {
                        EmployeeId = _employeeId,
                        Lat = loc.Coordinate.Latitude.ToString(),
                        Long = loc.Coordinate.Longitude.ToString(),
                        CreateDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        Time = DateTime.UtcNow.ToString("HH:mm:ss")
                    };

                    // Send directly to SignalR
                    _signalR.SendEmployeeLocation(data);

                    // Optionally still invoke callback if you want shared service to react
                    var args = new GeolocationLocationChangedEventArgs(
                        new Location(loc.Coordinate.Latitude, loc.Coordinate.Longitude)
                    );
                    callback?.Invoke(this, args);
                }
            };

            _manager.StartUpdatingLocation();
        }

        public void StopBackgroundTracking()
        {
            _manager?.StopUpdatingLocation();
        }
    }
}

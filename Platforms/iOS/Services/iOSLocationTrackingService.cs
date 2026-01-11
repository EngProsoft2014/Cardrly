using Cardrly.Models;
using Cardrly.Services;
using Cardrly.Services.Data;
using CoreLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Platforms.iOS.Services
{
    public class iOSLocationTrackingService : IPlatformLocationService
    {
        private CLLocationManager _manager;

        public void StartBackgroundTracking(string employeeId, EventHandler<GeolocationLocationChangedEventArgs> callback)
        {
            _manager = new CLLocationManager
            {
                AllowsBackgroundLocationUpdates = true,
                PausesLocationUpdatesAutomatically = false
            };

            _manager.RequestAlwaysAuthorization();
            _manager.DesiredAccuracy = CLLocation.AccuracyBest;
            _manager.DistanceFilter = 10;

            _manager.LocationsUpdated += (s, e) =>
            {
                var loc = e.Locations.LastOrDefault();
                if (loc != null)
                {
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

using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Pages;
using Cardrly.Services.AudioStream;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Services.Data
{
    public class LocationTrackingService
    {
        private readonly SignalRService _signalR;
        readonly ServicesService _service;
        readonly IGenericRepository Rep;
        private readonly IAudioStreamService _audioService;
        private readonly IPlatformLocationService _platformLocation;
        private bool _isListening;

        private string _employeeId;

        public LocationTrackingService(SignalRService signalR, IPlatformLocationService platformLocation, IGenericRepository GenericRep, ServicesService service, IAudioStreamService audioService)
        {
            _signalR = signalR;
            _service = service;
            Rep = GenericRep;
            _audioService = audioService;
            _platformLocation = platformLocation;
        }

        public async Task StartAsync(string employeeId)
        {
            if (_isListening)
                return; // or throw a controlled exception

            _isListening = true;

            var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            if (status != PermissionStatus.Granted)
            {
                await Toast.Make("Location permission not granted", CommunityToolkit.Maui.Core.ToastDuration.Long, 15).Show();
                return;
            }

            var request = new GeolocationListeningRequest(
                GeolocationAccuracy.High,
                TimeSpan.FromSeconds(10)
            );

            bool started = false;

            try
            {
                // Try Foreground listening first
                started = await Geolocation.StartListeningForegroundAsync(request);

                // If Foreground not available, fall back to background
                if (started)
                {
                    _employeeId = employeeId;
                    Geolocation.LocationChanged -= OnLocationChanged;
                    Geolocation.LocationChanged += OnLocationChanged;

                    Geolocation.ListeningFailed += async (s, e) =>
                    {
                        await Toast.Make($"⚠️ Location listening failed: {e.Error}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15).Show();
                        await App.Current!.MainPage!.Navigation.PushAsync(new NoGpsPage(Rep, _service, _signalR, _audioService, this));
                    };
                }
                else
                {
                    // If neither worked, delegate to platform native service
                    _platformLocation.StartBackgroundTracking(employeeId);
                }

            }
            catch (FeatureNotEnabledException)
            {
                await App.Current!.MainPage!.DisplayAlert(
                    "Alert",
                    "Please enable location services (GPS) in your device settings.",
                    "Ok");
            }
            catch (PermissionException)
            {
                await App.Current!.MainPage!.DisplayAlert(
                    "Permissions",
                    "GPS access was not granted",
                    "Ok");
            }
        }

        private async void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
        {
            var loc = e.Location;
            if (loc != null)
            {
                var data = new DataMapsModel
                {
                    EmployeeId = _employeeId,
                    AccountId = Preferences.Default.Get(ApiConstants.AccountId, string.Empty),
                    TimeSheetId = Preferences.Default.Get(ApiConstants.TimeSheetId, string.Empty),
                    Lat = loc.Latitude,
                    Long = loc.Longitude,
                    CreateDate = DateTime.UtcNow,
                    Time = DateTime.UtcNow.TimeOfDay
                };

                await _signalR.SendEmployeeLocation(data);
            }
        }

        public void Stop()
        {
            try
            {
                if (!_isListening)
                    return;

                _isListening = false;

                Geolocation.LocationChanged -= OnLocationChanged;
                Geolocation.StopListeningForeground();
            }
            catch { }

            // 🔥 THIS IS CRITICAL FOR iOS
            _platformLocation.StopBackgroundTracking();
        }

        public void StartBackgroundTrackingLocation(string employeeId, object value)
        {
            _platformLocation.StartBackgroundTracking(employeeId);
        }

        public void StopBackgroundTrackingLocation()
        {
            _platformLocation.StopBackgroundTracking();
        }
    }
}

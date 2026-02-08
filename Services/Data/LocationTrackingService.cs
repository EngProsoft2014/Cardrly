
using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Extensions;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.TimeSheet;
using Cardrly.Pages;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Plugin.FirebasePushNotifications;
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
        readonly IAudioStreamService _audioService;
        readonly IPlatformLocationService _platformLocation;
        readonly IFirebasePushNotification _firebasePushNotification;

        private readonly SemaphoreSlim _startStopLock = new(1, 1);

        private bool _isListening;
        private string _employeeId;

        public LocationTrackingService(SignalRService signalR, 
            IPlatformLocationService platformLocation, 
            IGenericRepository GenericRep,
            ServicesService service,
            IAudioStreamService audioService,
            IFirebasePushNotification firebasePushNotification)
        {
            _signalR = signalR;
            _service = service;
            Rep = GenericRep;
            _audioService = audioService;
            _platformLocation = platformLocation;
            _firebasePushNotification = firebasePushNotification;
        }

        public async Task StartAsync(string employeeId)
        {
            bool isCheckout = Preferences.Default.Get(ApiConstants.isTimeSheetCheckout, false);
            string userId = Preferences.Default.Get(ApiConstants.userid, string.Empty);

            if (string.IsNullOrEmpty(userId))
                return;

            if (StaticMember.CheckPermission(ApiConstants.SendLocationTimeSheet) && !isCheckout)
            {
                await _startStopLock.WaitAsync();

                try
                {
                    if (_isListening)
                        return;

                    var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                    if (status != PermissionStatus.Granted)
                        status = await Permissions.RequestAsync<Permissions.LocationAlways>();

                    if (status != PermissionStatus.Granted)
                    {
                        await Toast.Make(
                            "Location permission not granted",
                            CommunityToolkit.Maui.Core.ToastDuration.Long,
                            15).Show();
                        return;
                    }

                    var request = new GeolocationListeningRequest(
                        GeolocationAccuracy.High,
                        TimeSpan.FromSeconds(10));

                    bool started = await Geolocation.StartListeningForegroundAsync(request);

                    if (started)
                    {
                        _employeeId = employeeId;

                        Geolocation.LocationChanged -= OnLocationChanged;
                        Geolocation.LocationChanged += OnLocationChanged;

                        Geolocation.ListeningFailed -= OnListeningFailed;
                        Geolocation.ListeningFailed += OnListeningFailed;

                        _isListening = true; // ✅ set ONLY after success
                    }
                    else
                    {
                        _platformLocation.StartBackgroundTracking(employeeId);
                        _isListening = true;
                    }
                }
                catch (Exception)
                {
                    _isListening = false;
                }
                finally
                {
                    _startStopLock.Release();
                }
            }
        }
        private async void OnListeningFailed(object sender, GeolocationListeningFailedEventArgs e)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Toast.Make(
                    $"⚠️ Location listening failed: {e.Error}",
                    CommunityToolkit.Maui.Core.ToastDuration.Long,
                    15).Show();

                await App.Current!.MainPage!.Navigation.PushAsync(
                    new NoGpsPage(Rep, _service, _signalR, _audioService, this, _firebasePushNotification));
            });
        }

        private async void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
        {
            var loc = e.Location;
            if (loc == null) return;

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

        public async Task StopAsync()
        {
            await _startStopLock.WaitAsync();
            try
            {
                if (!_isListening)
                    return;

                _isListening = false;

                Geolocation.LocationChanged -= OnLocationChanged;
                Geolocation.ListeningFailed -= OnListeningFailed;

                Geolocation.StopListeningForeground();
                _platformLocation.StopBackgroundTracking();


                bool isCheckout = Preferences.Default.Get(ApiConstants.isTimeSheetCheckout, false);
                string accountId = Preferences.Default.Get(ApiConstants.AccountId, string.Empty);
                string CardId = Preferences.Default.Get(ApiConstants.cardId, string.Empty);
                if (string.IsNullOrEmpty(CardId))
                    return;

                if (StaticMember.CheckPermission(ApiConstants.SendLocationTimeSheet) && !isCheckout)
                {
                    string userToken = await _service.UserToken();

                    var json = await Rep.PostAsync(ApiConstants.SendNotifyToOwner + accountId + "/" + CardId, userToken);
                }
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        // Backward compatibility with your App.xaml.cs
        public void Stop() => StopAsync().FireAndForget();
        public void StopBackgroundTrackingLocation() => _platformLocation.StopBackgroundTracking();
        public void StartBackgroundTrackingLocation(string employeeId, object _)
        {
            bool isCheckout = Preferences.Default.Get(ApiConstants.isTimeSheetCheckout, false);
            string userId = Preferences.Default.Get(ApiConstants.userid, string.Empty);

            if (string.IsNullOrEmpty(userId))
                return;

            if (StaticMember.CheckPermission(ApiConstants.SendLocationTimeSheet) && !isCheckout)
            {
                _platformLocation.StartBackgroundTracking(employeeId);
            }
        }



        //StaticMember.notificatioLocationsManager.SendNotification(AppResources.titleLocationsharingstopped, AppResources.msgLocalNotificationforLocations, DateTime.Now.Date.AddSeconds(3));

    }
}

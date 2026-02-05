using Akavache;
using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Extensions;
using Cardrly.Helpers;
using Cardrly.Models.Permision;
using Cardrly.Pages;
using Cardrly.Services;
using Cardrly.ViewModels;
using Newtonsoft.Json;
using Plugin.Maui.Audio;
using System.Globalization;
using Cardrly.Services.Data;
using System.Reactive.Linq;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using Cardrly.Services.AudioStream;
using Controls.UserDialogs.Maui;
using System.Reactive.Concurrency;
using Cardrly.Pages.MainPopups;
using Cardrly.Models;
using Mopups.Services;
using CommunityToolkit.Mvvm.Messaging;
using Cardrly.Models.TimeSheet;
using Microsoft.IdentityModel.Tokens;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model;




#if ANDROID
using Cardrly.Platforms.Android;
#elif IOS
using Cardrly.Platforms.iOS;
#endif

namespace Cardrly
{
    public partial class App : Application
    {
        #region Service
        readonly IGenericRepository Rep;
        readonly ServicesService _service;
        readonly SignalRService _signalRService;
        readonly LocationTrackingService _locationTracking;
        bool _gpsPageShown = false;
        bool _locationStartRequested = false;

        public static IServiceProvider Services { get; private set; }
        private readonly IAudioStreamService _audioService;
        private readonly IFirebasePushNotification _firebasePushNotification;
        private readonly INotificationPermissions _notificationPermissions;
        #endregion

        public static bool isHaveTimeSheetTracking = false;
        public static bool UploadInProgress { get; set; } = false;
        public static bool IsInBackground { get; private set; }
        int NavToSecurePage = 0;
        public App(IGenericRepository GenericRep,
            ServicesService service,
            SignalRService signalRService,
            LocationTrackingService locationTracking,
            IAudioStreamService audioService,
            IServiceProvider serviceProvider,
            INotificationManagerService notificationManagerService,
            IFirebasePushNotification firebasePushNotification,
            INotificationPermissions notificationPermissions)
        {
            try
            {

                Rep = GenericRep;
                _service = service;
                Services = serviceProvider;
                _signalRService = signalRService;
                _locationTracking = locationTracking;
                _firebasePushNotification = firebasePushNotification;
                _notificationPermissions = notificationPermissions;
                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
                StaticMember.notificationManager = notificationManagerService;
                LoadSetting();
                _audioService = audioService;

                BlobCache.ApplicationName = "CardrlyDB";
                BlobCache.EnsureInitialized();
                // Register global exception handling
                GlobalExceptionHandler.RegisterGlobalExceptionHandlers();
                InitializeComponent();
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(ApiConstants.syncFusionLicence);

                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    // Connection to internet is Not available
                    MainPage = new NavigationPage(new NoInternetPage(Rep, _service, _signalRService, _audioService, _locationTracking));
                    return;
                }
                else
                {
                    // Subscribe to security status changes
                    string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    string Stringdate = Preferences.Default.Get(ApiConstants.ExpireDate, "");

                    bool IsExpireDate = DateOnly.TryParseExact(Stringdate, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly ExpireDate);
                    if (string.IsNullOrEmpty(AccountId) || ExpireDate < DateOnly.FromDateTime(DateTime.UtcNow) || IsExpireDate == false)
                    {
                        Preferences.Default.Clear();
                        MainPage = new NavigationPage(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
                    }
                    else
                    {
                        MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service, _signalRService, _audioService, _locationTracking), Rep, _service, _signalRService, _audioService, _locationTracking));
                    }

                    WeakReferenceMessenger.Default.Register<GpsStatusMessage>(this, (r, m) =>
                    {
                        if (!m.IsEnabled && !_gpsPageShown)
                        {
                            _gpsPageShown = true;
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await App.Current!.MainPage!.Navigation.PushAsync(
                                    new NoGpsPage(Rep, _service, _signalRService, _audioService, _locationTracking));
                            });
                        }
                        else if (m.IsEnabled)
                        {
                            _gpsPageShown = false; // reset when GPS is back
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Preferences.Default.Clear();
                MainPage = new NavigationPage(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
            }
        }

        private async void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess != NetworkAccess.Internet)
            {
                // Connection to internet is Not available
                await App.Current!.MainPage!.Navigation.PushAsync(new NoInternetPage(Rep, _service, _signalRService, _audioService, _locationTracking));
                return;
            }
        }

        protected async override void OnStart()
        {
            base.OnStart();

            AuthorizationStatus authorizationStatus = await _notificationPermissions.GetAuthorizationStatusAsync();

            await _firebasePushNotification.RegisterForPushNotificationsAsync();

            var token = _firebasePushNotification.Token;


            //await Task.WhenAll(GetDeviceIdFromDataBase(), StatusLocation(), SignalRservice(), CheckToStartSendLocation());
            await GetDeviceIdFromDataBase();
            await StatusLocation();
            await SignalRservice();
            await CheckToStartSendLocation();

            IsInBackground = false;

            MessagingCenter.Subscribe<object>(this, "AppForegrounded", async sender =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (UserDialogs.Instance != null && App.Current?.MainPage != null)
                    {
                        await Task.Delay(100); // wait for UI to be ready
                        UserDialogs.Instance.Loading("Resuming upload...", maskType: MaskType.Clear);
                    }
                });

                UploadInProgress = false;
            });

            MessagingCenter.Subscribe<object>(this, "AppBackgrounded", async sender =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (UserDialogs.Instance != null && App.Current?.MainPage != null)
                    {
                        await Task.Delay(50);
                        UserDialogs.Instance.HideHud();
                    }
                });
            });
        }

        protected async override void OnResume()
        {
            base.OnResume();

            _signalRService.OnMessageReceivedOneDeviceForThisAccount -= _signalRService_OnMessageReceivedOneDeviceForThisAccountInSleep;

            IsInBackground = false;

            string userId = Preferences.Default.Get(ApiConstants.userid, "");
            string Stringdate = Preferences.Default.Get(ApiConstants.ExpireDate, "");

            if (userId == "" && !Preferences.Get(ApiConstants.isFirstRun, true))
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
                await App.Current!.MainPage!.DisplayAlert(AppResources.lblAlert, AppResources.msgLougoutForLoginAnotherDevice, AppResources.msgOk);
                return;
            }

            if (!string.IsNullOrEmpty(Stringdate))
            {
                bool IsExpireDate = DateOnly.TryParseExact(Stringdate, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly ExpireDate);
                if (string.IsNullOrEmpty(Stringdate) || ExpireDate < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    await ClearMostData();
                    await App.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
                    return;
                }
            }

            //await Task.WhenAll(SignalRservice(), CheckToStartSendLocation());
            await SignalRservice();
            await CheckToStartSendLocation();

            if (UploadInProgress)
            {
                MessagingCenter.Send<object>(this, "AppForegrounded");
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            bool isCheckout = Preferences.Default.Get(ApiConstants.isTimeSheetCheckout, false);

            // 🔥 ALWAYS stop foreground
            _locationTracking.StopAsync();//only stop foureground location

            // 🔥 ALWAYS stop background first
            _locationTracking.StopBackgroundTrackingLocation();

            if (StaticMember.CheckPermission(ApiConstants.SendLocationTimeSheet) && !isCheckout)
            {
                _signalRService.OnMessageReceivedOneDeviceForThisAccount += _signalRService_OnMessageReceivedOneDeviceForThisAccountInSleep;

                string userId = Preferences.Default.Get(ApiConstants.userid, "");

                if (!string.IsNullOrEmpty(userId))
                {
                    _locationTracking.StartBackgroundTrackingLocation(userId, null);
                }
            }

            IsInBackground = true;
            MessagingCenter.Send<object>(this, "AppBackgrounded");
        }

        void LoadSetting()
        {
            string Lan = Preferences.Default.Get("Lan", "en");
            if (Lan == "ar")
            {
                CultureInfo cal = new CultureInfo("ar");
                TranslateExtension.Instance.SetCulture(cal);
            }
            else
            {
                CultureInfo cal = new CultureInfo("en");
                TranslateExtension.Instance.SetCulture(cal);
            }
        }

        async Task StatusLocation()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != null)
            {
                if (status != PermissionStatus.Granted)
                {
                    await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
            }
            else
            {
                status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                if (status != null && status != PermissionStatus.Granted)
                {
                    await Permissions.RequestAsync<Permissions.LocationAlways>();
                }
            }
        }

        async Task CheckToStartSendLocation()
        {
            if (_locationStartRequested)
                return;

            _locationStartRequested = true;

            try
            {
                bool isCheckout = Preferences.Default.Get(ApiConstants.isTimeSheetCheckout, false);

                if (isCheckout)
                {
                    await _locationTracking.StopAsync();
                    _locationTracking.StopBackgroundTrackingLocation();
                    return;
                }

                string userId = Preferences.Default.Get(ApiConstants.userid, string.Empty);

                if (string.IsNullOrEmpty(userId))
                    return;

                if (StaticMember.CheckPermission(ApiConstants.SendLocationTimeSheet) && !isCheckout)
                {
                    string deviceId = Preferences.Default.Get(ApiConstants.DeviceId, string.Empty);
                    string timeSheetId = Preferences.Default.Get(ApiConstants.TimeSheetId, string.Empty);
                    if (!string.IsNullOrEmpty(deviceId) && !string.IsNullOrEmpty(timeSheetId))
                    {
                        await _locationTracking.StartAsync(userId);
                        await EnsureGpsEnabled();
                    }
                    else
                    {
                        string cardId = Preferences.Default.Get(ApiConstants.cardId, string.Empty);
                        if (!string.IsNullOrEmpty(cardId))
                        {
                            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                            {
                                string UserToken = await _service.UserToken();

                                string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");
                                string CardId = Preferences.Default.Get(ApiConstants.cardId, "");

                                var json = await Rep.GetAsync<TimeSheetResponse>(ApiConstants.GetByCardIdTimeSheetApi + AccountId + "/" + CardId, UserToken);

                                if (json != null)
                                {
                                    if (json.HoursFrom != null && json.HoursTo == null)
                                    {
                                        string deviceID = await StaticMember.GetDeviceId();
                                        if (!string.IsNullOrEmpty(deviceID) && deviceID == json.DeviceId)
                                        {
                                            Preferences.Default.Set(ApiConstants.DeviceId, json.DeviceId);
                                            Preferences.Default.Set(ApiConstants.TimeSheetId, json.Id);

                                            Preferences.Default.Set(ApiConstants.isTimeSheetCheckout, false);//check in

                                            if (!string.IsNullOrEmpty(userId))
                                                await _locationTracking.StartAsync(userId);

                                            await EnsureGpsEnabled();
                                        }
                                    }
                                    else if (json.HoursFrom != null && json.HoursTo != null)
                                    {
                                        Preferences.Default.Set(ApiConstants.isTimeSheetCheckout, true);//check out
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                _locationStartRequested = false;
            }
        }

        async Task GetDeviceIdFromDataBase()
        {
            string accountId = Preferences.Default.Get(ApiConstants.AccountId, string.Empty);
            string cardId = Preferences.Default.Get(ApiConstants.cardId, string.Empty);

            if (string.IsNullOrEmpty(accountId) && string.IsNullOrEmpty(cardId))
                return;

            bool isCheckout = Preferences.Default.Get(ApiConstants.isTimeSheetCheckout, false);

            if (StaticMember.CheckPermission(ApiConstants.SendLocationTimeSheet) && !isCheckout)
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    string UserToken = await _service.UserToken();
                    string DeviceId = await Rep.GetAsync<string>(ApiConstants.GetDeviceIdTimeSheetApi + accountId + "/" + cardId, UserToken);
                    if (!string.IsNullOrEmpty(DeviceId))
                    {
                        string myDeviceId = Preferences.Default.Get(ApiConstants.DeviceId, string.Empty);
                        myDeviceId = string.IsNullOrEmpty(myDeviceId) ? StaticMember.GetDeviceId().Result : myDeviceId;

                        if (DeviceId != myDeviceId)
                        {
                            await ClearMostData();
                            await App.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
                            await App.Current!.MainPage!.DisplayAlert(AppResources.lblAlert, AppResources.msgLougoutForLoginAnotherDevice, AppResources.msgOk);
                            return;
                        }
                    }
                }
            }
        }

        public async Task SignalRservice()
        {
#if ANDROID
            UserDialogs.Instance.ShowLoading();
#endif
            try
            {
                await _signalRService.StartAsync();
            }
            catch (Exception ex)
            {
                // Retry after delay
                await Task.Delay(3000);
                await _signalRService.StartAsync();
            }

            // Prevent duplicate subscriptions
            _signalRService.OnMessageReceivedLogout -= _signalRService_OnMessageReceivedLogout;
            _signalRService.OnMessageReceivedUpdateVersion -= _signalRService_OnMessageReceivedUpdateVersion;
            _signalRService.OnMessageReceivedOneDeviceForThisAccount -= _signalRService_OnMessageReceivedOneDeviceForThisAccount;

            _signalRService.OnMessageReceivedLogout += _signalRService_OnMessageReceivedLogout;// Logout Changed Permissions           
            _signalRService.OnMessageReceivedUpdateVersion += _signalRService_OnMessageReceivedUpdateVersion;// UpdateVersion
            _signalRService.OnMessageReceivedOneDeviceForThisAccount += _signalRService_OnMessageReceivedOneDeviceForThisAccount; //OneDeviceForThisAccountLogout
#if ANDROID
            UserDialogs.Instance.HideHud();
#endif
        }



        //Location signalR
        //public async Task StartSignalRLocationservice()
        //{
        //    string UserId = Preferences.Default.Get(ApiConstants.userid, "");
        //    await _signalRService.StartLocationTrackingAsync(UserId);

        //    await EnsureGpsEnabled();
        //}

        private async Task EnsureGpsEnabled()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(1));
                var location = await Geolocation.GetLocationAsync(request);

                if (location == null)
                {
                    await App.Current!.MainPage!.Navigation.PushAsync(new NoGpsPage(Rep, _service, _signalRService, _audioService, _locationTracking));
                }
            }
            catch (FeatureNotEnabledException)
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new NoGpsPage(Rep, _service, _signalRService, _audioService, _locationTracking));
            }
            catch (PermissionException)
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new NoGpsPage(Rep, _service, _signalRService, _audioService, _locationTracking));
            }
        }


        async Task HandleNotify()
        {
#if ANDROID || IOS
            MessagingCenter.Subscribe<NotificationManagerService, int>(this, "NoifcationClicked", async (sender, message) =>
            {
                if (message == 2)
                {
                    //Controls.StaticMember.TabIndex = message;

                    await App.Current!.MainPage!.Navigation.PushAsync(new HomePage(new HomeViewModel(Rep, _service, _signalRService, _audioService, _locationTracking), Rep, _service, _signalRService, _audioService, _locationTracking));

                }
            });
#endif
        }


        // Logout
        [Obsolete]
        private async void _signalRService_OnMessageReceivedLogout(string GuidKey)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await ClearMostData();

                await _signalRService.InvokeNotifyDisconnectyAsync(GuidKey);

                await App.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
                await App.Current!.MainPage!.DisplayAlert(AppResources.msgWarning, AppResources.MsgloggedOut, AppResources.msgOk);
            });
        }


        // UpdateVersion
        [Obsolete]
        private async void _signalRService_OnMessageReceivedUpdateVersion(string GuidKey, string Name, string VersionNumber, string VersionBuild, string DescriptionEN, string DescriptionAR, string ReleaseDate)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                UpdateVersionModel oUpdateVersionModel = new UpdateVersionModel
                {
                    Name = Name,
                    VersionNumber = VersionNumber,
                    VersionBuild = VersionBuild,
                    Description = DescriptionEN,
                    DescriptionAr = DescriptionAR,
                    ReleaseDate = DateTime.Parse(ReleaseDate)
                };

                await ClearMostData();

                await MopupService.Instance.PushAsync(new UpdateVersionPopup(oUpdateVersionModel));
            });
        }

        [Obsolete]
        private async void _signalRService_OnMessageReceivedOneDeviceForThisAccount(string AccountId, string UserId, string CardId, string DeviceId)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                string accountId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string userId = Preferences.Default.Get(ApiConstants.userid, "");
                string cardId = Preferences.Default.Get(ApiConstants.cardId, "");

                string deviceID = await StaticMember.GetDeviceId();

                if (!string.IsNullOrEmpty(accountId) && AccountId == accountId && !string.IsNullOrEmpty(userId) && UserId == userId &&
                    !string.IsNullOrEmpty(cardId) && CardId == cardId && !string.IsNullOrEmpty(deviceID) && DeviceId != deviceID)
                {
                    await ClearMostData();

                    await Application.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));

                    await App.Current!.MainPage!.DisplayAlert(AppResources.lblAlert, AppResources.msgLougoutForLoginAnotherDevice, AppResources.msgOk);
                }
            });
        }

        [Obsolete]
        private async void _signalRService_OnMessageReceivedOneDeviceForThisAccountInSleep(string AccountId, string UserId, string CardId, string DeviceId)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                string accountId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string userId = Preferences.Default.Get(ApiConstants.userid, "");
                string cardId = Preferences.Default.Get(ApiConstants.cardId, "");

                string deviceID = await StaticMember.GetDeviceId();

                if (!string.IsNullOrEmpty(accountId) && AccountId == accountId && !string.IsNullOrEmpty(userId) && UserId == userId &&
                    !string.IsNullOrEmpty(cardId) && CardId == cardId && !string.IsNullOrEmpty(deviceID) && DeviceId != deviceID)
                {
                    await ClearMostData();
                }
            });
        }

        async Task ClearMostData()
        {
            await StaticMember.DeleteUserSession(Rep, _service);

            string LangValueToKeep = Preferences.Default.Get("Lan", "en");

            bool RememberMe = Preferences.Default.Get<bool>(ApiConstants.rememberMe, false);
            string RememberMeUserName = Preferences.Default.Get<string>(ApiConstants.rememberMeUserName, string.Empty);
            string RememberPassword = Preferences.Default.Get<string>(ApiConstants.rememberMePassword, string.Empty);

            Preferences.Default.Clear();
            await BlobCache.LocalMachine.InvalidateAll();
            await BlobCache.LocalMachine.Vacuum();

            Preferences.Default.Set("Lan", LangValueToKeep);
            Preferences.Default.Set(ApiConstants.rememberMe, RememberMe);
            Preferences.Default.Set(ApiConstants.rememberMeUserName, RememberMeUserName);
            Preferences.Default.Set(ApiConstants.rememberMePassword, RememberPassword);
        }
    }
}
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
        readonly Services.Data.ServicesService _service;
        private SignalRService _signalRService;
        public static IServiceProvider Services { get; private set; }
        #endregion
        int NavToSecurePage = 0;
        public App(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioManager audioManager, IServiceProvider serviceProvider,
            INotificationManagerService notificationManagerService)
        {
            try
            {
                Rep = GenericRep;
                _service = service;
                Services = serviceProvider;
                StaticMember.notificationManager = notificationManagerService;
                LoadSetting();
                Controls.StaticMember._audioManager = audioManager;
                BlobCache.ApplicationName = "CardrlyDB";
                BlobCache.EnsureInitialized();
                // Register global exception handling
                GlobalExceptionHandler.RegisterGlobalExceptionHandlers();
                InitializeComponent();
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(ApiConstants.syncFusionLicence);
                // Subscribe to security status changes
                string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string Stringdate = Preferences.Default.Get(ApiConstants.ExpireDate, "");
                if (!string.IsNullOrEmpty(Stringdate))
                {
                    DateOnly date = DateOnly.Parse(Stringdate);
                    if (string.IsNullOrEmpty(AccountId) || date < DateOnly.FromDateTime(DateTime.Now))
                    {
                        Preferences.Default.Clear();
                        MainPage = new NavigationPage(new LoginPage(new LoginViewModel(Rep, _service, audioManager)));
                    }
                    else
                    {
                        MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service, audioManager), Rep, _service));
                    }
                }
                else
                {
                    Preferences.Default.Clear();
                    MainPage = new NavigationPage(new LoginPage(new LoginViewModel(Rep, _service, audioManager)));
                }
                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            }
            catch (Exception ex)
            {
                // Maui Team 
            }
        }

        //private void OnSecurityStatusChanged(bool isSecure, string msg)
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        if (isSecure && NavToSecurePage == 0)
        //        {
        //            // Navigate to Block Screen if security is compromised
        //            App.Current!.MainPage!.Navigation.PushAsync(new Security_WarningPage(msg));
        //            NavToSecurePage = 1;
        //        }
        //        else if (!isSecure && NavToSecurePage == 1)
        //        {
        //            // Navigate back to the main app when secure again
        //            App.Current!.MainPage!.Navigation.PopAsync();
        //            NavToSecurePage = 0;
        //        }
        //    });
        //}

        private async void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess != NetworkAccess.Internet)
            {
                // Connection to internet is Not available
                await App.Current!.MainPage!.Navigation.PushAsync(new NoInternetPage(Rep, _service));
                return;
            }
        }

        protected async override void OnStart()
        {
            base.OnStart();
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Connection to internet is Not available
                await App.Current!.MainPage!.Navigation.PushAsync(new NoInternetPage(Rep, _service));
                return;
            }
            else
            {
                await SignalRservice();
            }
        }

        protected async override void OnResume()
        {
            base.OnResume();
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Connection to internet is Not available
                await App.Current!.MainPage!.Navigation.PushAsync(new NoInternetPage(Rep, _service));
                return;
            }
            // Ensure SignalR reconnects after coming from background
            await SignalRservice();

        }

        protected async override void OnSleep()
        {
            base.OnSleep();
            await SignalRNotservice();
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


        public async Task SignalRservice()
        {
            if (_signalRService == null)
            {
                _signalRService = new SignalRService(_service);
            }

            // Logout
            _signalRService.OnMessageReceivedLogout -= _signalRService_OnMessageReceivedLogout;
            _signalRService.OnMessageReceivedLogout += _signalRService_OnMessageReceivedLogout;

            // UpdateVersion
            _signalRService.OnMessageReceivedUpdateVersion -= _signalRService_OnMessageReceivedUpdateVersion;
            _signalRService.OnMessageReceivedUpdateVersion += _signalRService_OnMessageReceivedUpdateVersion;

            // Check if already connected
            if (_signalRService.IsConnected == false)
            {
                await _signalRService.StartAsync();
            }
        }

        public async Task SignalRNotservice()
        {
            if (_signalRService != null)
            {
                // Logout
                _signalRService.OnMessageReceivedLogout -= _signalRService_OnMessageReceivedLogout;

                // UpdateVersion
                _signalRService.OnMessageReceivedUpdateVersion -= _signalRService_OnMessageReceivedUpdateVersion;

                await _signalRService.Disconnect();

                _signalRService = null; // Ensure it's fully disposed
            }
        }

        // Logout
        private async void _signalRService_OnMessageReceivedLogout(string GuidKey)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                string LangValueToKeep = Preferences.Default.Get("Lan", "en");
                Preferences.Default.Clear();
                await BlobCache.LocalMachine.InvalidateAll();
                await BlobCache.LocalMachine.Vacuum();

                await _signalRService.InvokeNotifyDisconnectyAsync(GuidKey);

                Preferences.Default.Set("Lan", LangValueToKeep);
                await App.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, StaticMember._audioManager)));
                await App.Current!.MainPage!.DisplayAlert(AppResources.msgWarning, AppResources.MsgloggedOut, AppResources.msgOk);
            });
        }


        // UpdateVersion
        private async void _signalRService_OnMessageReceivedUpdateVersion(string GuidKey, string VersionNumber, string Description, string RealseDate)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {

            });

        }


        async Task HandleNotify()
        {
#if ANDROID || IOS
            MessagingCenter.Subscribe<NotificationManagerService, int>(this, "NoifcationClicked", async (sender, message) =>
            {
                if (message == 2)
                {
                    //Controls.StaticMember.TabIndex = message;

                    await App.Current!.MainPage!.Navigation.PushAsync(new HomePage(new HomeViewModel(Rep, _service, StaticMember._audioManager), Rep, _service));

                }
            });
#endif

        }
    }
}
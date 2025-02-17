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
        public static IServiceProvider Services { get; private set; }
        #endregion
        public App(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioManager audioManager, IServiceProvider serviceProvider,
            INotificationManagerService  notificationManagerService)
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
                string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");

                if (string.IsNullOrEmpty(AccountId))
                {
                    MainPage = new NavigationPage(new LoginPage(new LoginViewModel(Rep, _service, audioManager)));
                }
                else
                {
                    MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service, audioManager), Rep, _service));
                }
                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            }
            catch(Exception ex)
            {
                // Maui Team 
            }
        }

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
                //await HandleNotify();
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
            else
            {
                //await HandleNotify();
            }
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
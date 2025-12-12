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
        private readonly IAudioStreamService _audioService;
        #endregion

        public static bool UploadInProgress { get; set; } = false;
        public static bool IsInBackground { get; private set; }
        int NavToSecurePage = 0;
        public App(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioStreamService audioService, IServiceProvider serviceProvider,
            INotificationManagerService notificationManagerService)
        {
            try
            {

                Rep = GenericRep;
                _service = service;
                Services = serviceProvider;
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
                    MainPage = new NavigationPage(new NoInternetPage(Rep, _service, _audioService));
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
                        MainPage = new NavigationPage(new LoginPage(new LoginViewModel(Rep, _service, _audioService)));
                    }
                    else
                    {
                        MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service), Rep, _service, _audioService));
                    }

                    //// Subscribe only for this upload
                    //MessagingCenter.Subscribe<object>(this, "AppForegrounded", sender =>
                    //{
                    //    MainThread.BeginInvokeOnMainThread(() =>
                    //    {
                    //        UserDialogs.Instance.Loading("Resuming upload...", maskType: MaskType.Clear);
                    //    });

                    //    UploadInProgress = false;
                    //});

                    //MessagingCenter.Subscribe<object>(this, "AppBackgrounded", sender =>
                    //{
                    //    // Optionally hide the HUD when app goes background
                    //    MainThread.BeginInvokeOnMainThread(() =>
                    //    {
                    //        UserDialogs.Instance.HideHud();
                    //    });
                    //});
                }

            }
            catch (Exception ex)
            {
                Preferences.Default.Clear();
                MainPage = new NavigationPage(new LoginPage(new LoginViewModel(Rep, _service, _audioService)));
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
                await App.Current!.MainPage!.Navigation.PushAsync(new NoInternetPage(Rep, _service, _audioService));
                return;
            }
        }

        protected async override void OnStart()
        {
            base.OnStart();
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
            //if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            //{
            //    // Connection to internet is Not available
            //    await App.Current!.MainPage!.Navigation.PushAsync(new NoInternetPage(Rep, _service));
            //    return;
            //}
        }

        protected async override void OnResume()
        {
            base.OnResume();
            IsInBackground = false;
            if (UploadInProgress)
            {
                MessagingCenter.Send<object>(this, "AppForegrounded");      
            }

            string Stringdate = Preferences.Default.Get(ApiConstants.ExpireDate, "");
            if (!string.IsNullOrEmpty(Stringdate))
            {
                bool IsExpireDate = DateOnly.TryParseExact(Stringdate, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly ExpireDate);
                if (string.IsNullOrEmpty(Stringdate) || ExpireDate < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    string LangValueToKeep = Preferences.Default.Get("Lan", "en");
                    Preferences.Default.Clear();
                    await BlobCache.LocalMachine.InvalidateAll();
                    await BlobCache.LocalMachine.Vacuum();

                    Preferences.Default.Set("Lan", LangValueToKeep);
                    await App.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _audioService)));
                }
            }

        }

        protected async override void OnSleep()
        {
            base.OnSleep();
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


        async Task HandleNotify()
        {
#if ANDROID || IOS
            MessagingCenter.Subscribe<NotificationManagerService, int>(this, "NoifcationClicked", async (sender, message) =>
            {
                if (message == 2)
                {
                    //Controls.StaticMember.TabIndex = message;

                    await App.Current!.MainPage!.Navigation.PushAsync(new HomePage(new HomeViewModel(Rep, _service), Rep, _service, _audioService));

                }
            });
#endif

        }
    }
}
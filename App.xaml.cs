using Akavache;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Pages;
using Cardrly.ViewModels;
using Plugin.Maui.Audio;
using System.Globalization;

namespace Cardrly
{
    public partial class App : Application
    {
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion
        public App(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioManager audioManager)
        {
            try
            {
                Preferences.Default.Set("Lan", "ar");
                CultureInfo.CurrentCulture = new CultureInfo("ar");
                CultureInfo.CurrentUICulture = new CultureInfo("ar");
                Rep = GenericRep;
                _service = service;
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
                    var page = new HomePage(new HomeViewModel(Rep, _service, audioManager), Rep, _service);
                    MainPage = new NavigationPage(page);
                }
            }
            catch(Exception ex)
            {
                // Maui Team 
            }
        }

    }
}
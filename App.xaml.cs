


using Akavache;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Pages;
using Cardrly.ViewModels;

namespace Cardrly
{
    public partial class App : Application
    {
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion
        public App(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            try
            {
                Rep = GenericRep;
                _service = service;

                BlobCache.ApplicationName = "CardrlyDB";
                BlobCache.EnsureInitialized();

                InitializeComponent();
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(ApiConstants.syncFusionLicence);
                string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");

                if (string.IsNullOrEmpty(AccountId))
                {
                    MainPage = new NavigationPage(new LoginPage(new LoginViewModel(Rep, _service)));
                }
                else
                {
                    var page = new HomePage(new HomeViewModel(Rep, _service), Rep, _service);
                    MainPage = new NavigationPage(page);
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
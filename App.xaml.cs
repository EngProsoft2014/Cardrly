


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
        [Obsolete]
        public App(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(ApiConstants.syncFusionLicence);
            string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");

            if (string.IsNullOrEmpty(AccountId))
            {
                var vm = new LoginViewModel(Rep, _service);
                var page = new LoginPage();
                page.BindingContext = vm;
                MainPage = new NavigationPage(page);
            }
            else
            {
                var vm = new HomeViewModel();
                var page = new HomePage(Rep, _service);
                page.BindingContext = vm;
                MainPage = new NavigationPage(page);
            }

        }
    }
}
using Akavache;
using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Pages;
using Cardrly.Pages.MainPopups;
using Cardrly.Pages.MeetingsScript;
using Cardrly.Pages.TrackingPages;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using Cardrly.Services.Data;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Plugin.Maui.Audio;
using System.Reactive.Linq;

namespace Cardrly.ViewModels
{
    public partial class MoreViewModel : BaseViewModel
    {
        [ObservableProperty]
        bool isShowBullingInfo;


        private readonly IAudioStreamService _audioService;
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        readonly SignalRService _signalRService;
        readonly LocationTrackingService _locationTracking;
        #endregion

        #region Cons
        public MoreViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service, SignalRService signalRService, IAudioStreamService audioService, LocationTrackingService locationTracking)
        {
            Rep = GenericRep;
            _service = service;
            _signalRService = signalRService;
            _audioService = audioService;
            _locationTracking = locationTracking;
            IsShowBullingInfo = StaticMember.CheckPermission(ApiConstants.GetStripe) == true ? true : false;
            
        }
        #endregion

        #region RelayCommand

        [RelayCommand]
        Task LogoutClick()
        {
            Action action = async () =>
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
                await Application.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
            };
            Controls.StaticMember.ShowSnackBar($"{AppResources.msgDoYouWantToLogout}", Controls.StaticMember.SnackBarColor, Controls.StaticMember.SnackBarTextColor, action);
            return Task.CompletedTask;
        }

        [RelayCommand]
        async Task AccountInfoClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new AccountInfoPage(new AccountInfoViewModel(Rep, _service)));
        }
        [RelayCommand]
        async Task ChangePasswordClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new ChangePasswordPage(new ChangePasswordViewModel(Rep,_service, _signalRService, _audioService, _locationTracking)));
        }
        //[RelayCommand]
        //async Task TimeSheetClick()
        //{
        //    await App.Current!.MainPage!.Navigation.PushAsync(new TimeSheetPage(new TimeSheetViewModel(Rep, _service)));
        //}

        [RelayCommand]
        async Task AdOnsPageClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new AdOnsPage(new ADOnsViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
        }

        //[RelayCommand]
        //async Task TrackingClick()
        //{
        //    await App.Current!.MainPage!.Navigation.PushAsync(new EmployeesWorkingPage(new EmployeesViewModel(Rep, _service),Rep,_service));
        //}
        [RelayCommand]
        async Task ActiveDeviceClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new ActiveDevicePage(new ActiveDeviceViewModel(Rep, _service)));
        }
        [RelayCommand]
        async Task DeviceClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new DevicesPage(new DevicesViewModel(Rep, _service)));
        }
        
        [RelayCommand]
        async Task LanguageClick()
        {
            await MopupService.Instance.PushAsync(new LanguagePopup(Rep, _service, _signalRService, _audioService, _locationTracking));
        }
        [RelayCommand]
        async Task FAQClick()
        {
            try
            {
                Uri uri = new Uri("https://cardrly.com/faq/");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        [RelayCommand]
        async Task HelpClick()
        {
            try
            {
                Uri uri = new Uri("https://cardrly.com/contact-us/");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        [RelayCommand]
        async Task BillingClick()
        {
            if (StaticMember.CheckPermission(ApiConstants.GetStripe))
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new BillingPage(new BillingViewModel(Rep, _service)));
            }
            else
            {
                var toast = Toast.Make($"{AppResources.mshPermissionToViewData}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            
        }
        [RelayCommand]
        async Task DocumentsClick()
        {
            try
            {
                Uri uri = new Uri("https://cardrly.com/docs/");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        [RelayCommand]
        async Task ShopClick()
        {
            try
            {
                Uri uri = new Uri("https://cardrly.com/shop/");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        #endregion
    }
}

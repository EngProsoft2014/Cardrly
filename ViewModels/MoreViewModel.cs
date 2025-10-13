using Akavache;
using CommunityToolkit.Mvvm.Input;
using Cardrly.Helpers;
using Cardrly.Pages;
using System.Reactive.Linq;
using Plugin.Maui.Audio;
using Cardrly.Resources.Lan;
using Mopups.Services;
using Cardrly.Pages.MainPopups;
using CommunityToolkit.Maui.Alerts;
using Cardrly.Controls;
using Controls.UserDialogs.Maui;
using Cardrly.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using Cardrly.Services.AudioStream;
using Cardrly.Pages.MeetingsScript;
using Cardrly.Pages.TrackingPages;

namespace Cardrly.ViewModels
{
    public partial class MoreViewModel : BaseViewModel
    {
        [ObservableProperty]
        bool isShowBullingInfo;
        [ObservableProperty]
        bool isShowMeetingSCript;

        private readonly IAudioStreamService _audioService;
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public MoreViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioStreamService audioService)
        {
            Rep = GenericRep;
            _service = service;
            _audioService = audioService;

            IsShowBullingInfo = StaticMember.CheckPermission(ApiConstants.GetStripe) == true ? true : false;
            IsShowMeetingSCript = StaticMember.CheckPermission(ApiConstants.GetMeetingAi) == true ? true : false;
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
                Preferences.Default.Clear();
                await BlobCache.LocalMachine.InvalidateAll();
                await BlobCache.LocalMachine.Vacuum();

                Preferences.Default.Set("Lan", LangValueToKeep);
                await Application.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _audioService)));
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
            await App.Current!.MainPage!.Navigation.PushAsync(new ChangePasswordPage(new ChangePasswordViewModel(Rep,_service, _audioService)));
        }
        [RelayCommand]
        async Task TimeSheetClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new TimeSheetPage(new TimeSheetViewModel(Rep, _service)));
        }
        [RelayCommand]
        async Task TrackingClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new EmployeesWorkingPage(new EmployeesViewModel(Rep, _service),Rep,_service));
        }
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
        async Task MeetingsScriptClick()
        {
            if (StaticMember.CheckPermission(ApiConstants.GetMeetingAi))
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new NotesScriptPage(new NotesScriptViewModel(Rep, _service, _audioService)));
            }
            else
            {
                var toast = Toast.Make($"{AppResources.mshPermissionToViewData}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        [RelayCommand]
        async Task LanguageClick()
        {
            await MopupService.Instance.PushAsync(new LanguagePopup(Rep, _service, _audioService));
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

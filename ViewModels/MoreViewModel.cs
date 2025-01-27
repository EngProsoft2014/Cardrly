using Akavache;
using CommunityToolkit.Mvvm.Input;
using Cardrly.Helpers;
using Cardrly.Pages;
using System.Reactive.Linq;
using Plugin.Maui.Audio;
using Cardrly.Resources.Lan;
using Mopups.Services;
using Cardrly.Pages.MainPopups;

namespace Cardrly.ViewModels
{
    public partial class MoreViewModel : BaseViewModel
    {
        readonly IAudioManager _audioManager;
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public MoreViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioManager audioManager)
        {
            Rep = GenericRep;
            _service = service;
            // Initialize audio manager
            _audioManager = audioManager;
        }
        #endregion

        #region RelayCommand

        [RelayCommand]
        Task ExitClick()
        {
            Action action = async () =>
            {
                string LangValueToKeep = Preferences.Default.Get("Lan", "en");
                Preferences.Default.Clear();
                await BlobCache.LocalMachine.InvalidateAll();
                await BlobCache.LocalMachine.Vacuum();

                Preferences.Default.Set("Lan", LangValueToKeep);
                await Application.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service,_audioManager)));
            };
            Controls.StaticMember.ShowSnackBar($"{AppResources.msgDoYouWantToLogout}", Controls.StaticMember.SnackBarColor, Controls.StaticMember.SnackBarTextColor, action);
            return Task.CompletedTask;
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
        async Task LanguageClick()
        {
            await MopupService.Instance.PushAsync(new LanguagePopup(Rep, _service));
        }
        #endregion
    }
}

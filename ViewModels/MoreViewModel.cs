using Akavache;
using CommunityToolkit.Mvvm.Input;
using Cardrly.Helpers;
using Cardrly.Pages;
using Plugin.NFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

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
        async Task ProfileClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new ProfilePage(new ProfileViewModel(Rep, _service)));
        }

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
            Controls.StaticMember.ShowSnackBar("Do you want to Logout", Controls.StaticMember.SnackBarColor, Controls.StaticMember.SnackBarTextColor, action);
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
        #endregion
    }
}

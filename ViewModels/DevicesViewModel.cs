
using Cardrly.Constants;
using Cardrly.Enums;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Models.Devices;
using Cardrly.Pages;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Microsoft.AspNet.SignalR.Client;
using Mopups.Services;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class DevicesViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        ObservableCollection<DevicesResponse> devices = new ObservableCollection<DevicesResponse>();
        [ObservableProperty]
        public CardDetailsResponse detailsResponse = new CardDetailsResponse(); 
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public DevicesViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        }
        #endregion

        #region Methods
        async void Init()
        {
            UserDialogs.Instance.ShowLoading();
            await GetAccountCard();
            await GetAllDevices();
            UserDialogs.Instance.HideHud();
        }

        async Task GetAccountCard()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string UserId = Preferences.Default.Get(ApiConstants.userid, "");
                var json = await Rep.GetAsync<CardDetailsResponse>($"{ApiConstants.CardGetByUserApi}{AccId}/Card/User/{UserId}", UserToken);
                if (json != null)
                {
                    DetailsResponse = json;
                }
            }
            IsEnable = true;
        }

        public async Task GetAllDevices()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string UserId = Preferences.Default.Get(ApiConstants.userid, "");
                var json = await Rep.GetAsync<ObservableCollection<DevicesResponse>>($"{ApiConstants.DevicesGetByCardApi}{AccId}/Card/{DetailsResponse.Id}/Devices", UserToken);
                if (json != null)
                {
                    Devices = json;
                }
            }
            IsEnable = true;
        }

        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task ActiveDeviceClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new ActiveDevicePage(new ActiveDeviceViewModel(Rep, _service)));
        }
        [RelayCommand]
        async Task DeletDeviceClick(DevicesResponse res)
        {
            bool result = await App.Current!.MainPage!.DisplayAlert($"{AppResources.msgDeleteDevice}" , $"{AppResources.msgDeleteDevice_qu}", $"{AppResources.msgYes}", $"{AppResources.msgNo}");
            if (result)
            {
                UserDialogs.Instance.ShowLoading();
                IsEnable = false;
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    string response = await Rep.PostEAsync($"{ApiConstants.DevicesDeleteApi}{AccId}/Card/{DetailsResponse.Id}/Devices/{res.Id}/Delete", UserToken);
                    if (response == "")
                    {
                        UserDialogs.Instance.ShowLoading();
                        await GetAllDevices();
                        UserDialogs.Instance.HideHud();
                    }
                    else
                    {
                        var toast = Toast.Make($"{AppResources.msgTheDeviceHasNotBeenDeleted_}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                IsEnable = true;
                UserDialogs.Instance.HideHud();
            }
        }
        [RelayCommand]
        async Task CancelClick()
        {
            await App.Current!.MainPage!.Navigation.PopAsync();
        } 
        #endregion
    }
}

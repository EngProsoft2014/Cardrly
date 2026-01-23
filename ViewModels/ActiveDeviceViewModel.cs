using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Models.Card;
using Cardrly.Models;
using Cardrly.Models.Devices;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class ActiveDeviceViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        ObservableCollection<DevicesTypeModel> deviceModels = new ObservableCollection<DevicesTypeModel>();
        [ObservableProperty]
        public CardDetailsResponse detailsResponse = new CardDetailsResponse();
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public ActiveDeviceViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        }
        #endregion

        #region Methods
        async void Init()
        {
            await GetAccountCard();
            await GetDevices();
        }
        async Task GetAccountCard()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string UserId = Preferences.Default.Get(ApiConstants.userid, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<CardDetailsResponse>($"{ApiConstants.CardGetByUserApi}{AccId}/Card/User/{UserId}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    DetailsResponse = json;
                }
            }
            IsEnable = true;
        }
        async Task GetDevices()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string UserId = Preferences.Default.Get(ApiConstants.userid, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<ObservableCollection<DevicesTypeModel>>($"{ApiConstants.DevicesGetAllApi}{AccId}/Card/{DetailsResponse.Id}/Devices/LstDevices", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    DeviceModels = json;
                }
            }
            IsEnable = true;
        }
        public async Task DeviceClick(string uriRedirect, int deviceType, string DeviceId)
        {

            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                var reqdto = new DevicesRequest
                {
                    DeviceType = deviceType,
                    RedirectUrl = uriRedirect,
                    DeviceId = DeviceId
                };
                var res = await Rep.PostTRAsync<DevicesRequest, DevicesResponse>($"{ApiConstants.DevicesAddApi}{AccId}/Card/{DetailsResponse.Id}/Devices", reqdto, UserToken);
                if (res.Item1 != null)
                {
                    var toast = Toast.Make($"{AppResources.msgDeviceAddedSuccessfully}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else
                {
                    var toast = Toast.Make($"{res.Item2!.errors!.FirstOrDefault().Key + " " + res.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();

                }
                
            }
            IsEnable = true;
        }
        #endregion
    }
}

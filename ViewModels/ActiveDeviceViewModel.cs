using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Models;
using Cardrly.Models.Devices;
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
        ObservableCollection<ActivateDeviceModel> deviceModels = new ObservableCollection<ActivateDeviceModel>();
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
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "Card",
                Image = "",
                Type = "FontIconBrand"
            });
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "Band",
                Image = "",
                Type = "FontIconSolid"
            });
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "Display",
                Image = "",
                Type = "FontIconSolid"
            });
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "KeyChain",
                Image = "",
                Type = "FontIconSolid"
            });
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "Google stand",
                Image = "",
                Type = "FontIconSolid"
            });
            await GetAccountCard();
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
                    DetailsResponse.CardUrl = $"https://app.cardrly.com/profile/{json.Id}";
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
                var json = await Rep.GetAsync<CardDetailsResponse>($"{ApiConstants.CardGetByUserApi}{AccId}/Card/User/{UserId}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    DetailsResponse = json;
                    DetailsResponse.CardUrl = $"https://app.cardrly.com/profile/{json.Id}";
                }
            }
            IsEnable = true;
        }
        public async Task DeviceClick()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                var reqdto = new DevicesRequest
                {
                    DeviceType = Enums.EnumDeviceType.Band,
                    RedirectUrl = DetailsResponse.CardUrl!,
                    DeviceId = "12348957890ABCDEF"
                };
                var res = await Rep.PostTRAsync<DevicesRequest, DevicesResponse>($"{ApiConstants.DevicesAddApi}{AccId}/Card/{DetailsResponse.Id}/Devices", reqdto, UserToken);
                if (res.Item1 != null)
                {

                }
                else
                {
                    {
                        var toast = Toast.Make($"{res.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                IsEnable = true;
            }
        }
        #endregion
    }
}

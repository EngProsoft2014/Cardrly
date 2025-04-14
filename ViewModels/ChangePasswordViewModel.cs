using Azure.Core;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;

namespace Cardrly.ViewModels
{
    public partial class ChangePasswordViewModel : BaseViewModel
    {
        [ObservableProperty]
        ChangePasswordModel model = new ChangePasswordModel();
        [ObservableProperty]
        string confirmPassword = "";

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public ChangePasswordViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
        }
        #endregion


        [RelayCommand]
        async Task SaveClick()
        {
            if (string.IsNullOrEmpty(Model!.currentPassword))
            {
                var toast = Toast.Make($"{AppResources.RFCurrentPassword}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else if (string.IsNullOrEmpty(Model!.newPassword))
            {
                var toast = Toast.Make($"{AppResources.FRNewPassword}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else if (string.IsNullOrEmpty(ConfirmPassword))
            {
                var toast = Toast.Make($"{AppResources.FRConfirmNewPassword}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else if (Model.newPassword != ConfirmPassword)
            {
                var toast = Toast.Make($"{AppResources.msgNew_Password_Doesn_t_Match_Confirm_New_Password}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    string UserToken = await _service.UserToken();
                    string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                    UserDialogs.Instance.ShowLoading();
                    var Postjson = await Rep.PostTRAsync<ChangePasswordModel, ErrorResult>($"{ApiConstants.UserChangePasswordApi}", Model!, UserToken);
                    UserDialogs.Instance.HideHud();
                    if (Postjson.Item2 == null)
                    {
                        await App.Current!.MainPage!.Navigation.PopAsync();
                    }
                    else
                    {
                        var toast = Toast.Make($"{Postjson.Item2!.errors!.Values.FirstOrDefault()}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
            }
        }
    }
}

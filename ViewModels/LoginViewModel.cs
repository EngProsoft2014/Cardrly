using Akavache;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Cardrly.Helpers;
using Cardrly.Mode_s.ApplicationUser;
using Cardrly.Pages;
using Cardrly.Services.Data;
using System.Reactive.Linq;
using Cardrly.Constants;

namespace Cardrly.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        public ApplicationUserLoginRequest loginRequest = new ApplicationUserLoginRequest();
        [ObservableProperty]
        public ApplicationUserResponse userResponse = new ApplicationUserResponse();
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public LoginViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task LoginClick(ApplicationUserLoginRequest model)
        {
            IsEnable = false;
            if (string.IsNullOrEmpty(model.UserName))
            {
                var toast = Toast.Make($"Field Required : User Name", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else if (string.IsNullOrEmpty(model.Password))
            {
                var toast = Toast.Make($"Field Required : Password", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.PostTRAsync<ApplicationUserLoginRequest, ApplicationUserResponse>(ApiConstants.LoginApi, model);

                if (json.Item1 != null)
                {
                    UserResponse = json.Item1;

                    if (!string.IsNullOrEmpty(UserResponse?.Id))
                    {
                        Controls.StaticMember.WayOfTab = 0;

                        Preferences.Default.Set(ApiConstants.userid, UserResponse.Id);
                        Preferences.Default.Set(ApiConstants.email, UserResponse.Email);
                        Preferences.Default.Set(ApiConstants.username, UserResponse.UserName);
                        Preferences.Default.Set(ApiConstants.userPermision, UserResponse.UserPermision);
                        Preferences.Default.Set(ApiConstants.userCategory, UserResponse.UserCategory);
                        Preferences.Default.Set(ApiConstants.AccountId, UserResponse.AccountId);

                        await BlobCache.LocalMachine.InsertObject(ServicesService.UserTokenServiceKey, UserResponse?.Token, DateTimeOffset.Now.AddMinutes(43200));

                        var page = new HomePage(new HomeViewModel(Rep, _service), Rep,_service);
                        await App.Current!.MainPage!.Navigation.PushAsync(page);
                    }
                }
                else
                {

                    var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }

                UserDialogs.Instance.HideHud();
            }
            IsEnable = true;
        }
        #endregion
    }
}

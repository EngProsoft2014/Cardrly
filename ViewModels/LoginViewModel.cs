using Akavache;
using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Models.ApplicationUser;
using Cardrly.Models;
using Cardrly.Pages;
using Cardrly.Pages.MainPopups;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using Cardrly.Services.Data;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Newtonsoft.Json;
using Plugin.FirebasePushNotifications;
using Plugin.Maui.Audio;
using System.IdentityModel.Tokens.Jwt;
using System.Reactive.Linq;


namespace Cardrly.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        public ApplicationUserLoginRequest loginRequest = new ApplicationUserLoginRequest();
        [ObservableProperty]
        public ApplicationUserResponse userResponse = new ApplicationUserResponse();
        private readonly IAudioStreamService _audioService;
        [ObservableProperty]
        bool isRememberMe = false;
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        readonly SignalRService _signalRService;
        readonly LocationTrackingService _locationTracking;
        private readonly IFirebasePushNotification _firebasePushNotification;
        #endregion

        #region Cons
        public LoginViewModel(IGenericRepository GenericRep, 
            Services.Data.ServicesService service, 
            SignalRService signalRService, 
            IAudioStreamService audioService, 
            LocationTrackingService locationTracking,
            IFirebasePushNotification firebasePushNotification)
        {
            Rep = GenericRep;
            _service = service;
            _signalRService = signalRService;
            _audioService = audioService;
            _locationTracking = locationTracking;
            _firebasePushNotification = firebasePushNotification;
            Init();
        }
        #endregion

        #region Methods
        void Init()
        {
            if (Preferences.Default.Get<bool>(ApiConstants.rememberMe, false))
            {
                IsRememberMe = true;
                LoginRequest.UserName = Preferences.Default.Get<string>(ApiConstants.rememberMeUserName, string.Empty);
                LoginRequest.Password = Preferences.Default.Get<string>(ApiConstants.rememberMePassword, string.Empty);
            }
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task LoginClick(ApplicationUserLoginRequest model)
        {
            IsEnable = false;
            if (string.IsNullOrEmpty(model.UserName))
            {
                var toast = Toast.Make($"{AppResources.msgFRUserName}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else if (string.IsNullOrEmpty(model.Password))
            {
                var toast = Toast.Make($"{AppResources.msgFRPassword}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                UserDialogs.Instance.ShowLoading();

                if (IsRememberMe)
                {
                    Preferences.Default.Set(ApiConstants.rememberMe, true);
                    Preferences.Default.Set(ApiConstants.rememberMeUserName, model.UserName);
                    Preferences.Default.Set(ApiConstants.rememberMePassword, model.Password);
                }
                else
                {
                    Preferences.Default.Set(ApiConstants.rememberMe, false);
                    Preferences.Default.Remove(ApiConstants.rememberMeUserName);
                    Preferences.Default.Remove(ApiConstants.rememberMePassword);
                }

                await _firebasePushNotification.RegisterForPushNotificationsAsync();
                model.FCM_Token = _firebasePushNotification.Token;

                model.DeviceId = await StaticMember.GetDeviceId();

                var json = await Rep.PostTRAsync<ApplicationUserLoginRequest, ApplicationUserResponse>(ApiConstants.LoginApi, model);

                if (json.Item1 != null)
                {
                    UserResponse = json.Item1;
                    if (UserResponse.Account != null)
                    {
                        if (UserResponse.Account.ExpireDateAcc >= DateOnly.FromDateTime(DateTime.UtcNow))
                        {
                            if (!string.IsNullOrEmpty(UserResponse?.Id))
                            {
                                if (UserResponse.VersionAppMobile != null && UserResponse.VersionAppMobile.Count > 0)
                                {
                                    int currentVersionParse = int.Parse(AppInfo.VersionString.Replace(".", ""));
                                    int currentBuildParse = int.Parse(AppInfo.BuildString.Replace(".", ""));

                                    //Android
                                    if (DeviceInfo.Platform == DevicePlatform.Android)
                                    {
                                        var version = UserResponse.VersionAppMobile.Find(f => f.Name.ToLower() == "android");

                                        if (version != null)
                                        {
                                            int VersionNumberParse = int.Parse(version!.VersionNumber.Trim().Replace(".", ""));
                                            int VersionBuildParse = int.Parse(version!.VersionBuild.Trim().Replace(".", ""));

                                            if (currentBuildParse < VersionBuildParse)
                                            {
                                                await MopupService.Instance.PushAsync(new UpdateVersionPopup(version));
                                            }
                                            else
                                            {
                                                await SetData(UserResponse);
                                                var page = new HomePage(new HomeViewModel(Rep, _service, _signalRService, _audioService, _locationTracking, _firebasePushNotification), Rep, _service, _signalRService, _audioService, _locationTracking, _firebasePushNotification);
                                                await App.Current!.MainPage!.Navigation.PushAsync(page);
                                            }
                                        }
                                        else
                                        {
                                            await MopupService.Instance.PushAsync(new UpdateVersionPopup(new UpdateVersionModel()));
                                        }

                                    }
                                    else if (DeviceInfo.Platform == DevicePlatform.iOS) // iOS
                                    {
                                        var version = UserResponse.VersionAppMobile.Find(f => f.Name.ToLower() == "ios");

                                        if (version != null)
                                        {
                                            int VersionNumberParse = int.Parse(version!.VersionNumber.Trim().Replace(".", ""));
                                            int VersionBuildParse = int.Parse(version!.VersionBuild.Trim().Replace(".", ""));

                                            if (currentVersionParse < VersionNumberParse)
                                            {
                                                await MopupService.Instance.PushAsync(new UpdateVersionPopup(version));
                                            }
                                            else
                                            {
                                                await SetData(UserResponse);
                                                var page = new HomePage(new HomeViewModel(Rep, _service, _signalRService, _audioService, _locationTracking, _firebasePushNotification), Rep, _service, _signalRService, _audioService, _locationTracking, _firebasePushNotification);
                                                await App.Current!.MainPage!.Navigation.PushAsync(page);
                                            }
                                        }
                                        else
                                        {
                                            await MopupService.Instance.PushAsync(new UpdateVersionPopup(new UpdateVersionModel()));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var toast = Toast.Make($"{AppResources.msgAccountHasExpired}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                    }
                    else
                    {
                        var toast = Toast.Make($"{AppResources.msgDontGetAccount}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
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
        [RelayCommand]
        async Task ForgotPasswordClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new ResetPasswordPage(new ResetPasswordViewModel(Rep, _service)));
        }

        public async Task SetData(ApplicationUserResponse userResponse)
        {
            StaticMember.WayOfTab = 0;

            Preferences.Default.Set(ApiConstants.userid, userResponse.Id);
            Preferences.Default.Set(ApiConstants.email, userResponse.Email);
            Preferences.Default.Set(ApiConstants.username, userResponse.UserName);
            Preferences.Default.Set(ApiConstants.userPermision, JsonConvert.SerializeObject(userResponse.Permissions));
            Preferences.Default.Set(ApiConstants.userCategory, userResponse.UserCategory);
            Preferences.Default.Set(ApiConstants.AccountId, userResponse.AccountId);
            Preferences.Default.Set(ApiConstants.AccountName, userResponse.Account!.Name);
            Preferences.Default.Set(ApiConstants.ExpireDate, Convert.ToString(userResponse.Account!.ExpireDateAcc));
            Preferences.Set(ApiConstants.isFirstRun, false);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(userResponse.Token);

            var ownerIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "OwnerId")?.Value;
            if (!string.IsNullOrEmpty(ownerIdClaim))
                Preferences.Default.Set(ApiConstants.ownerId, ownerIdClaim);

            var cardIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "CardId")?.Value;
            if (!string.IsNullOrEmpty(cardIdClaim))
                Preferences.Default.Set(ApiConstants.cardId, cardIdClaim);

            await BlobCache.LocalMachine.InsertObject(ServicesService.UserTokenServiceKey, userResponse?.Token, DateTimeOffset.Now.AddMinutes(43200));

            Preferences.Default.Set(ApiConstants.GuidKey, Controls.StaticMember.GuidKeyFromToken(userResponse?.Token!));
        }


        [RelayCommand]
        async Task SignUpClick()
        {
            try
            {
                Uri uri = new Uri("https://cardrly.com/pricing/");
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

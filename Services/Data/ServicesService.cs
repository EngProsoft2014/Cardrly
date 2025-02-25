using Akavache;
using System.Reactive.Linq;
using Cardrly.Mode_s.ApplicationUser;
using Cardrly.Constants;
using Controls.UserDialogs.Maui;
using CommunityToolkit.Maui.Alerts;



namespace Cardrly.Services.Data
{
    public interface IServicesService
    {
        Task<ImageSource> AccountPhoto();
        Task<string> UserToken();
    }


    public class ServicesService : IServicesService
    {

        readonly Helpers.IGenericRepository Rep;
        public ServicesService(Helpers.IGenericRepository ORep)
        {
            Rep = ORep;
        }

        ImageSource Photo;
        string ServiceKey = "ServiceKey";

        //EmployeeModel loginModel;
        string MUserToken;
        public static string UserTokenServiceKey = "UserTokenServiceKey";

        public async Task<ImageSource> AccountPhoto()
        {
            //Photo = Controls.StaticMembers.AccountImg;
            await BlobCache.LocalMachine.InsertObject(ServiceKey, Photo, DateTimeOffset.Now.AddYears(1));
            return Photo;
        }


        public async Task<string> UserToken()
        {
            try
            {
                MUserToken = await BlobCache.LocalMachine.GetObject<string>(UserTokenServiceKey);
            }
            catch (Exception)
            {
                MUserToken = null;
            }

            if (MUserToken == null)
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {        
                    if (!string.IsNullOrEmpty(Preferences.Default.Get(ApiConstants.username, "")))
                    {
                        string Pass = await App.Current!.MainPage!.DisplayPromptAsync(Resources.Lan.AppResources.Info, Resources.Lan.AppResources.Your_Token_expired_Please_Enter_Your_Password, Cardrly.Resources.Lan.AppResources.msgOk , Resources.Lan.AppResources.btnCancel);

                        ApplicationUserLoginRequest model = new ApplicationUserLoginRequest()
                        {
                            UserName = Preferences.Default.Get(ApiConstants.username, ""),
                            Password = Pass
                        };

                        UserDialogs.Instance.ShowLoading();
                        var loginModel = await Rep.PostTRAsync<ApplicationUserLoginRequest, ApplicationUserResponse>(ApiConstants.LoginApi, model);
                        UserDialogs.Instance.HideHud();

                        if (loginModel.Item1 != null)
                        {
                            MUserToken = loginModel.Item1.Token!;

                            await BlobCache.LocalMachine.InsertObject(UserTokenServiceKey, loginModel.Item1.Token!, DateTimeOffset.Now.AddMinutes(43200));

                            return loginModel.Item1.Token!;
                        }
                        else
                        {
                            var toast = Toast.Make(Resources.Lan.AppResources.PasswordInvalid, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                    }
                }
                else
                {
                    //await App.Current!.MainPage!.Navigation.PushAsync(new NoInternetPage(Rep, this));
                }
            }
            else
            {
                return MUserToken;
            }

            return MUserToken!;
        }
    }
}

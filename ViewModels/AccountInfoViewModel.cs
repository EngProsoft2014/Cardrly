using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Account;
using Cardrly.Models.ApplicationUser;
using Cardrly.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using System.Threading.Tasks;


namespace Cardrly.ViewModels
{
    public partial class AccountInfoViewModel : BaseViewModel
    {
        [ObservableProperty]
        public ApplicationUserProfileResponse accountData = new ApplicationUserProfileResponse();

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public AccountInfoViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        } 
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task OpenFullScreenProfilePhoto(string image)
        {
            IsEnable = false;
            await App.Current!.MainPage!.Navigation.PushAsync(new FullScreenImage(image));
            IsEnable = true;
        }
        #endregion

        #region Method
        async void Init()
        {
            await GetAccountData();
        }
        async Task GetAccountData()
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<ApplicationUserProfileResponse>($"{ApiConstants.UserProfileApi}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    json.UserName = json.UserName.Contains("@") ? json.UserName.Split('@')[0]! : json.UserName!;
                    json.CompanyUrlLogo = string.IsNullOrEmpty(json.CompanyUrlLogo) ? "usericon.png" : Utility.ServerUrl + json.CompanyUrlLogo;
                    AccountData = json;
                }
            }
        } 
        #endregion
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using Controls.UserDialogs.Maui;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.ApplicationUser;
using Cardrly.Mode_s.Account;

namespace Cardrly.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        #region prop
        [ObservableProperty]
        public AccountResponse userResponse = new AccountResponse(); 
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public ProfileViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        }
        #endregion

        #region Methods
        async void Init()
        {
            await GetProfileData();
        }
        async Task GetProfileData()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<AccountResponse>($"{ApiConstants.AccountInfoApi}{AccId}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    json.UrlLogo = Utility.ServerUrl + json.UrlLogo;
                    json.ExpireProgress = json.DayOperationAcc - json.DayOperationExpireAcc;
                    UserResponse = json;
                }
            }
            IsEnable = true;
        }

        #endregion
  
    }
}

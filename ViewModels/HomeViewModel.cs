using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Account;
using Cardrly.Mode_s.Card;
using Cardrly.Models.Home;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        CardDashBoardResponse boardResponse = new CardDashBoardResponse();
        [ObservableProperty]
        AccountResponse accResponse = new AccountResponse();
        [ObservableProperty]
        public ObservableCollection<CardResponse> cardLst = new ObservableCollection<CardResponse>();
        [ObservableProperty]
        CardResponse selectedCard = new CardResponse();
        [ObservableProperty]
        DateTime fromDate = DateTime.UtcNow.Date;
        [ObservableProperty]
        DateTime toDate = DateTime.UtcNow.Date;
        public readonly IAudioManager _audioManager;
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public HomeViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioManager audioManager)
        {
            Rep = GenericRep;
            _service = service;
            // Initialize audio manager
            _audioManager = audioManager;
            Init();
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task GetClick()
        {
            if (FromDate.Date >ToDate.Date)
            {
                var toast = Toast.Make("The To Date Must be greater Than From Date", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                UserDialogs.Instance.ShowLoading();
                await GetAllCards();
                await GetAllStatistics();
                await GetAccData();
                UserDialogs.Instance.HideHud();
            }
        }

        #endregion

        #region Methodes
        public async void Init()
        {
            IsEnable = false;
            await GetAllCards();
            if (CardLst.Count > 0)
            {
                SelectedCard = CardLst[0];
            }
            UserDialogs.Instance.ShowLoading();
            await Task.WhenAll(GetAllStatistics(),GetAccData());
            UserDialogs.Instance.HideHud();
            IsEnable = true;
        }

        async Task GetAllStatistics()
        {
            
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                
                var json = await Rep.GetAsync<CardDashBoardResponse>($"{ApiConstants.CardGetCardDashBoardApi}{AccId}/Card/{SelectedCard.Id}/{FromDate.ToString("yyyy-MM-dd")}/{ToDate.ToString("yyyy-MM-dd")}", UserToken);
                
                if (json != null)
                {
                    if (json.clickCardLinkSummariesOS.Count == 0)
                    {
                        json.clickCardLinkSummariesOS.Add(new ClickCardSummaryOSResponse { name = "Empty", value=1});
                    }
                    BoardResponse = json;
                }
            }
            
        }

        async Task GetAccData()
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                
                var json = await Rep.GetAsync<AccountResponse>($"{ApiConstants.AccountInfoApi}{AccId}", UserToken);
                
                if (json != null)
                {
                    AccResponse = json;
                    AccResponse.CardProgress = ((json.CurrentCountCards / (double)json.CountCards) * 100);
                    AccResponse.UsersProgress = Convert.ToInt32((json.CurrentCountUsers / (double)json.CountUsers) * 100);
                    AccResponse.ExpireProgress = (json.DayOperationExpireAcc / (double)json.DayOperationAcc) * 100;
                }
            }
        }

        async Task GetAllCards()
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                
                var json = await Rep.GetAsync<ObservableCollection<CardResponse>>($"{ApiConstants.CardGetAllApi}{AccId}/Card", UserToken);
                
                if (json != null && json.Count > 0)
                {
                    CardLst = json;
                }
            }
        }
        #endregion
    }
}

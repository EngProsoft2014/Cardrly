using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Account;
using Cardrly.Mode_s.Card;
using Cardrly.Models.Home;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
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
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public HomeViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
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

        [RelayCommand]
        async Task SelectedDate()
        {
            IsEnable = false;
            var popupView = new Pages.MainPopups.DatePopup();
            popupView.RangeClose += (calendar) =>
            {
                UserDialogs.Instance.ShowLoading();
                calendar.StartDate.Value.ToString("MM-dd-yyyy");
                calendar.EndDate.Value.ToString("MM-dd-yyyy");
                FromDate = calendar.StartDate.Value;
                ToDate = calendar.EndDate.Value;
                UserDialogs.Instance.HideHud();
            };

            await MopupService.Instance.PushAsync(popupView);
            IsEnable = true;
        }
        #endregion

        #region Methodes
        public async void Init()
        {
            await GetAllCards();
            if (CardLst.Count > 0)
            {
                SelectedCard = CardLst[0];
            }
            UserDialogs.Instance.ShowLoading();
            await GetAllStatistics();
            await GetAccData();
            UserDialogs.Instance.HideHud();
        }

        async Task GetAllStatistics()
        {
            
            IsEnable = false;
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
            IsEnable = true;
            
        }

        async Task GetAccData()
        {
            
            IsEnable = false;
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
            IsEnable = true;
            
        }

        async Task GetAllCards()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                
                var json = await Rep.GetAsync<ObservableCollection<CardResponse>>($"{ApiConstants.CardGetAllApi}{AccId}/Card", UserToken);
                
                if (json != null)
                {
                    foreach (CardResponse card in json)
                    {
                        card.UrlImgProfile = Utility.ServerUrl + card.UrlImgProfile;
                        card.UrlImgCover = Utility.ServerUrl + card.UrlImgCover;
                        card.CardUrl = $"https://app.cardrly.com/profile/{card.Id}";
                    }
                    CardLst = json;
                }
            }
            IsEnable = true;
        }
        #endregion
    }
}

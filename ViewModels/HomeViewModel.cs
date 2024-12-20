using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Models.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using Controls.UserDialogs.Maui;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        [ObservableProperty]
        CardDashBoardResponse boardResponse = new CardDashBoardResponse();
        [ObservableProperty]
        public ObservableCollection<CardResponse> cardLst = new ObservableCollection<CardResponse>();
        [ObservableProperty]
        CardResponse selectedCard = new CardResponse();
        [ObservableProperty]
        DateTime fromDate = DateTime.UtcNow.Date;
        [ObservableProperty]
        DateTime toDate = DateTime.UtcNow.Date;
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

        #region Methodes
        public async void Init()
        {
            await GetAllCards();
            SelectedCard = CardLst[0];
            await GetAllStatistics();
        }

        async Task GetAllStatistics()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<CardDashBoardResponse>($"{ApiConstants.CardGetCardDashBoardApi}{AccId}/Card/{SelectedCard.Id}/{FromDate.ToString("yyyy-MM-dd")}/{ToDate.ToString("yyyy-MM-dd")}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    BoardResponse = json;
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
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<ObservableCollection<CardResponse>>($"{ApiConstants.CardGetAllApi}{AccId}/Card", UserToken);
                UserDialogs.Instance.HideHud();
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

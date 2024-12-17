using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Pages;
using Cardrly.Pages.MainPopups;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class CardsViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        ObservableCollection<CardResponse> cardLst = new ObservableCollection<CardResponse>(); 
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public CardsViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        } 
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task AddCardClick()
        {
            await MopupService.Instance.PushAsync(new UserPopup(Rep,_service));
        }
        [RelayCommand]
        async Task CardPreViewClick(CardResponse card)
        {
            var vm = new CardPreViewViewModel(card);
            var page = new CardPreViewPage(card);
            page.BindingContext = vm;
            await App.Current!.MainPage!.Navigation.PushAsync(page);
        }
        [RelayCommand]
        async Task ShareCardClick()
        {
            await MopupService.Instance.PushAsync(new QrCodePopup());
        }

        [RelayCommand]
        async Task MoreOPtionsClick(CardResponse card)
        {
            await MopupService.Instance.PushAsync(new CardOptionPopup(card, Rep, _service));
        }

        [RelayCommand]
        async Task EditCardClick(CardResponse card)
        {
            var vm = new AddCustomCardViewModel(card,Rep,_service);
            var page = new AddCustomCardPage(vm);
            page.BindingContext = vm;
            await App.Current!.MainPage!.Navigation.PushAsync(page);
        }
        #endregion

        #region Methodes
        public async void Init()
        {
            await GetAllCards();
        }

        async Task GetAllCards()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId,"");
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

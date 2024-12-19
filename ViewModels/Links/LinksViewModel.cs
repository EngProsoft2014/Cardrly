using Cardrly.Constants;
using Cardrly.Enums;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Mode_s.CardLink;
using Cardrly.Pages.Links;
using Cardrly.Pages.MainPopups;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;

namespace Cardrly.ViewModels.Links
{
    public partial class LinksViewModel : BaseViewModel
    {
        [ObservableProperty]
        CardDetailsResponse cardDetails = new CardDetailsResponse();
        public string CardId;
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion
        
        #region Cons
        public LinksViewModel(string cardId,IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            _service = service;
            Rep = GenericRep;
            CardId = cardId;
            Init(cardId);
        }
        #endregion
        
        #region RelayCommand
        [RelayCommand]
        async Task SelectLinkClick(CardLinkResponse cardLink)
        {
            var page = new EditLinkPopup(cardLink,Rep,_service);
            await MopupService.Instance.PushAsync(page);
        }
        [RelayCommand]
        public async Task ActiveClick(CardLinkResponse res)
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                IsEnable = false;
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                string ressponse = await Rep.PostEAsync($"{ApiConstants.CardLinkToggleApi}{AccId}/Card/{res.CardId}/CardLink/{res.Id}/ToggleActive", UserToken);
                if (ressponse == "")
                {
                    Init(res.CardId);
                }
                else
                {
                    var toast = Toast.Make($"Failed to change status", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                IsEnable = true;
                UserDialogs.Instance.HideHud();
            }
        }

        [RelayCommand]
        async Task DeletClick(CardLinkResponse res)
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                string response = await Rep.PostEAsync($"{ApiConstants.CardDeleteApi}{AccId}/Card/{res.CardId}/CardLink/{res.Id}/Delete", UserToken);
                if (response == "")
                {
                    Init(res.CardId);
                }
                else
                {
                    var toast = Toast.Make($"Failed to Delete Link", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                UserDialogs.Instance.HideHud();
            }
            IsEnable = true;
        }

        [RelayCommand]
        async Task AddLink()
        {
            var vm = new AddLinkViewModel(CardDetails.Id,Rep,_service);
            var page = new AddLinksPage();
            page.BindingContext = vm;
            await App.Current!.MainPage!.Navigation.PushAsync(page);
        }
        #endregion

        #region Methodes
        public async void Init(string CardId)
        {
            await GetAllCards(CardId);
        }

        async Task GetAllCards(string CardId)
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<CardDetailsResponse>($"{ApiConstants.CardGetDetailsAllApi}{CardId}", UserToken);
                
                if (json != null)
                {
                    foreach (CardLinkResponse cardLink in json.CardLinks)
                    {
                        cardLink.AccountLinkUrlImgName = Utility.ServerUrl + cardLink.AccountLinkUrlImgName;
                    }
                    CardDetails = json;
                }
                UserDialogs.Instance.HideHud();
            }
            IsEnable = true;
        }
        
        #endregion
    }
}

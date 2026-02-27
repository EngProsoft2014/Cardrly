using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Card;
using Cardrly.Models.CardLink;
using Cardrly.Pages.Links;
using Cardrly.Pages.MainPopups;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
namespace Cardrly.ViewModels.Links
{
    public partial class LinksViewModel : BaseViewModel
    {
        #region Prop
        // Main List
        [ObservableProperty]
        public CardDetailsResponse cardDetails = new CardDetailsResponse();
        // Just For order
        [ObservableProperty]
        public List<CardLinkResponse> cardOrder = new List<CardLinkResponse>();
        public string CardId; 
        #endregion

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
            await MopupService.Instance.PushAsync(new EditLinkPopup(cardLink, Rep, _service));
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
                    var toast = Toast.Make($"{AppResources.msgFailedToChangeStatus}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
                    var toast = Toast.Make($"{AppResources.msgFailedToChangeStatus}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                UserDialogs.Instance.HideHud();
            }
            IsEnable = true;
        }

        [RelayCommand]
        async Task AddLink()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new AddLinksPage(new AddLinkViewModel(CardDetails.Id, Rep, _service)));
        }
        #endregion

        #region Methodes
        public async void Init(string CardId)
        {
            await GetAllCards(CardId);
            MessagingCenter.Subscribe<EditLinkPopup, bool>(this, "CreateLink", async (sender, message) =>
            {

                if (true)
                {
                    await GetAllCards(CardId);
                }
            });
        }

        async Task GetAllCards(string CardId)
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<CardDetailsResponse>($"{ApiConstants.CardGetApi}{AccId}/Card/{CardId}", UserToken);     
                if (json != null)
                {
                    CardDetails = json;
                    CardOrder = new List<CardLinkResponse>(json.CardLinks);
                }
                UserDialogs.Instance.HideHud();
            }
            IsEnable = true;
        }

        public async Task OrderList(string CardId, List<CardLinkSortRequest> sortRequest)
        {
            UserDialogs.Instance.ShowLoading();
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (UserToken != null)
            {
                string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                var json = await Rep.PostTRAsync<List<CardLinkSortRequest>, List<CardLinkResponse>>($"{ApiConstants.CardLinkSortApi}{accid}/Card/{CardId}/CardLink/Resort", sortRequest, UserToken);
                if (json.Item1 != null)
                {
                    CardOrder = json.Item1;
                    var toast = Toast.Make($"{AppResources.msgSuccessfullyReordered}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (json.Item2 != null)
                {
                    var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }
            IsEnable = true;
            UserDialogs.Instance.HideHud();
        }
        #endregion
    }
}

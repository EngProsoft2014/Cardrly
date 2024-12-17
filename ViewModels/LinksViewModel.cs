using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Mode_s.CardLink;
using CommunityToolkit.Mvvm.ComponentModel;
using Controls.UserDialogs.Maui;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class LinksViewModel : BaseViewModel
    {
        [ObservableProperty]
        CardDetailsResponse cardDetails = new CardDetailsResponse();
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public LinksViewModel(string CardId,IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            _service = service;
            Rep = GenericRep;
            Init(CardId);
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
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    foreach (CardLinkResponse cardLink in json.CardLinks)
                    {
                        cardLink.AccountLinkUrlImgName = Utility.ServerUrl + cardLink.AccountLinkUrlImgName;
                    }
                    CardDetails = json;
                }
            }
            IsEnable = true;
        }
        #endregion
    }
}

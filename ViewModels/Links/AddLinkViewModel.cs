using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.AccountLinks;
using Cardrly.Pages.MainPopups;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels.Links
{
    public partial class AddLinkViewModel : BaseViewModel
    {
        [ObservableProperty]
        ObservableCollection<LinksGroup> accountLinks = new ObservableCollection<LinksGroup>();
        string CardId;
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public AddLinkViewModel(string cardId,IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            _service = service;
            Rep = GenericRep;
            CardId = cardId;
            Init();
        }
        #endregion

        [RelayCommand]
        async Task SelectClick(AccountLinkResponse res)
        {
            var page = new EditLinkPopup(1,res.Id,CardId,Rep,_service);
            await MopupService.Instance.PushAsync(page);
        }

        #region Methodes
        public async void Init()
        {
            await GetAllLinks();
        }

        async Task GetAllLinks()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<ObservableCollection<AccountLinkResponse>>($"{ApiConstants.AccountLinksCurrentApi}{AccId}/AccountLink/current", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    foreach (AccountLinkResponse res in json)
                    {
                        
                        res.UrlImgName = Utility.ServerUrl + res.UrlImgName;
                    }
                    AccountLinks.Add(new LinksGroup("Contact", json.Where(a => a.TypeLink == Enums.EnumTypeLink.Contact).ToList()));
                    AccountLinks.Add(new LinksGroup("SocialMedia", json.Where(a => a.TypeLink == Enums.EnumTypeLink.SocialMedia).ToList()));
                    AccountLinks.Add(new LinksGroup("Business", json.Where(a => a.TypeLink == Enums.EnumTypeLink.Business).ToList()));
                    
                }
            }
            IsEnable = true;
        }

        #endregion
    }
}

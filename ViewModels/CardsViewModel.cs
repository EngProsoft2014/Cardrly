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
using CommunityToolkit.Maui.Alerts;
using Cardrly.Controls;
using Cardrly.Resources.Lan;

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
            if (StaticMember.CheckPermission(ApiConstants.AddCards))
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new AddCustomCardPage(new AddCustomCardViewModel(Rep, _service)));
            }
            else
            {
                var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }

        }
        [RelayCommand]
        async Task CardPreViewClick(CardResponse card)
        {
            try
            {
                Uri uri = new Uri(card.CardUrlVM!);
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }

        [RelayCommand]
        async Task MoreOPtionsClick(CardResponse card)
        {
            await MopupService.Instance.PushAsync(new CardOptionPopup(card, Rep, _service));
        }

        [RelayCommand]
        async Task EditCardClick(CardResponse card)
        {
            if (StaticMember.CheckPermission(ApiConstants.UpdateCards))
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new AddCustomCardPage(new AddCustomCardViewModel(card, Rep, _service)));
            }
            else
            {
                var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }

        [RelayCommand]
        async Task OpenFullScreenProfilePhoto(string image)
        {
            IsEnable = false;
            if(image != "https://api.cardrly.com/")
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new FullScreenImage(image));
            }
            IsEnable = true;
        }

        [RelayCommand]
        async Task OpenFullScreenCoverPhoto(string image)
        {
            IsEnable = false;
            if (image != "https://api.cardrly.com/")
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new FullScreenImage(image));
            }
            IsEnable = true;
        }
        #endregion

        #region Methodes
        public async void Init()
        {
            if (StaticMember.CheckPermission(ApiConstants.GetCards))
            {
                await GetAllCards();
            }
            else
            {
                var toast = Toast.Make($"{AppResources.mshPermissionToViewData}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            //DeleteCard
            MessagingCenter.Subscribe<CardOptionPopup, bool>(this, "DeleteCard", async (sender, message) =>
            {

                if (true)
                {
                    await GetAllCards();
                }
            });

            //AddCard
            MessagingCenter.Subscribe<AddCustomCardViewModel, bool>(this, "AddCard", async (sender, message) =>
            {

                if (true)
                {
                    await GetAllCards();
                }
            });
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
                    CardLst = json;
                }
            }
            IsEnable = true;
        }
        #endregion
    }
}

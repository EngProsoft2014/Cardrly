using Controls.UserDialogs.Maui;
using Mopups.Services;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.ViewModels;
using System.Collections.ObjectModel;

namespace Cardrly.Pages.MainPopups;

public partial class CardOptionPopup : Mopups.Pages.PopupPage
{
    CardResponse Card = new CardResponse();

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public CardOptionPopup(CardResponse _Card, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        Card = _Card;
    }

    private void TapGestureRecognizer_SetupCard(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        this.IsEnabled = true;
    }

    private async void TapGestureRecognizer_EditCaed(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        var vm = new AddCustomCardViewModel(Card, Rep, _service);
        var page = new AddCustomCardPage();
        page.BindingContext = vm;
        await App.Current!.MainPage!.Navigation.PushAsync(page);
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }

    private async void TapGestureRecognizer_PreviewCard(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        var vm = new CardPreViewViewModel(Card);
        var page = new CardPreViewPage(Card);
        page.BindingContext = vm;
        await App.Current!.MainPage!.Navigation.PushAsync(page);
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }

    private async void TapGestureRecognizer_DeleteCard(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        string UserToken = await _service.UserToken();
        if (!string.IsNullOrEmpty(UserToken))
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            UserDialogs.Instance.ShowLoading();
            await Rep.DeleteAsync($"{ApiConstants.CardDeleteApi}{AccId}/Card/{Card.Id}/Delete", UserToken);
            UserDialogs.Instance.HideHud();
        }
        this.IsEnabled = true;
    }

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }
}
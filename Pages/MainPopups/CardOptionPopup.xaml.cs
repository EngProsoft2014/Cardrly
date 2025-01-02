using Controls.UserDialogs.Maui;
using Mopups.Services;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.ViewModels;
using System.Collections.ObjectModel;
using Plugin.NFC;
using System.Text;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System;
using CommunityToolkit.Maui.Alerts;

namespace Cardrly.Pages.MainPopups;

public partial class CardOptionPopup : Mopups.Pages.PopupPage
{
    CardResponse Card = new CardResponse();

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion

    #region Cons
    public CardOptionPopup(CardResponse _Card, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        Card = _Card;
    }
    #endregion

    #region Methods
    private async void TapGestureRecognizer_ShareCard(object sender, TappedEventArgs e)
    {
        await Share.RequestAsync(new ShareTextRequest
        {
            Uri = Card.CardUrl,
            Title = "Share Web Link"
        });
    }
    private async void TapGestureRecognizer_EditCaed(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        var vm = new AddCustomCardViewModel(Card, Rep, _service);
        var page = new AddCustomCardPage(vm);
        page.BindingContext = vm;
        await App.Current!.MainPage!.Navigation.PushAsync(page);
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }

    private async void TapGestureRecognizer_PreviewCard(object sender, TappedEventArgs e)
    {
        try
        {
            Uri uri = new Uri(Card.CardUrl!);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }

    private async void TapGestureRecognizer_DeleteCard(object sender, TappedEventArgs e)
    {
        bool ans = await DisplayAlert("Question", "Are you sure to delete This Card", "Ok", "Cancel");
        if (ans)
        {
            this.IsEnabled = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                await Rep.PostEAsync($"{ApiConstants.CardDeleteApi}{AccId}/Card/{Card.Id}/Delete", UserToken);
                UserDialogs.Instance.HideHud();
                await MopupService.Instance.PopAsync();
            }
            this.IsEnabled = true;
        }
    }

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    async void Button_Clicked_StartWriting_Uri(object sender, System.EventArgs e)
    {
        await MopupService.Instance.PopAsync();
        await MopupService.Instance.PushAsync(new ReadyToScanPopup(Card));
    }
    #endregion
   
}
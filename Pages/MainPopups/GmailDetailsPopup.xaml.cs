using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Globalization;
using System.Threading.Tasks;
using static Cardrly.Models.Calendar.GmailResponseModel;

namespace Cardrly.Pages.MainPopups;

public partial class GmailDetailsPopup : Mopups.Pages.PopupPage
{
    CalendarEventGmail Model;
    string CardId = "";
    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public GmailDetailsPopup(CalendarEventGmail model, string cardId, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        CardId = cardId;
        Rep = GenericRep;
        _service = service;
        if (model.ConferenceData is null)
        {
            model.ConferenceData = new ConferenceData();
            model.ConferenceData!.EntryPoints = new List<EntryPoint>();
        }
        this.BindingContext = model;
        Model = model;

        string Lan = Preferences.Default.Get("Lan", "en");

        if (Lan == "ar")
        {
            this.FlowDirection = FlowDirection.RightToLeft;
            CultureInfo.CurrentCulture = new CultureInfo("ar");
        }
        else
        {
            this.FlowDirection = FlowDirection.LeftToRight;
            CultureInfo.CurrentCulture = new CultureInfo("en");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        FontImageSource fontImageSource = new FontImageSource
        {
            Glyph = "", // Unicode for FontAwesome trash icon
            FontFamily = "FontIcon",
            Size = 20,
            Color = Colors.Red
        };

        imgDelete.Source = fontImageSource;
    }

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void JoinNow_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Uri uri = new Uri(Model.ConferenceData.EntryPoints[0].Uri!);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }

    //Delete Event
    public async void Delete_Tapped(object sender, TappedEventArgs e)
    {
        bool result = await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgDeleteEvent}", $"{AppResources.msgYes}", $"{AppResources.msgNo}");
        if (result)
        {
            await MopupService.Instance.PopAsync();
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                UserDialogs.Instance.ShowLoading();
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string response = await Rep.PostEAsync($"{ApiConstants.CalendarDeleteEventsApi}{AccId}/Calendar/CalendarType/1/DeleteEvent/{Model.Id}?CardId={CardId}", UserToken);
                UserDialogs.Instance.HideHud();
                if (response == "")
                {                   
                    MessagingCenter.Send(this, "DeleteEvent", true);
                }
                else
                {
                    var toast = Toast.Make($"{AppResources.msgTheEventHasNotBeenDeleted}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }
        }
    }
}
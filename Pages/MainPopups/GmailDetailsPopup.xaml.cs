using CommunityToolkit.Maui.Alerts;
using Mopups.Services;
using static Cardrly.Models.Calendar.GmailResponseModel;

namespace Cardrly.Pages.MainPopups;

public partial class GmailDetailsPopup : Mopups.Pages.PopupPage
{
    CalendarEventGmail Model;
    public GmailDetailsPopup(CalendarEventGmail model)
	{
		InitializeComponent();
        this.BindingContext = model;
        Model = model;
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
}
using CommunityToolkit.Maui.Alerts;
using Mopups.Services;
using System.Globalization;
using static Cardrly.Models.Calendar.OutLookResponseModel;

namespace Cardrly.Pages.MainPopups;

public partial class OutLookDetailsPopup : Mopups.Pages.PopupPage
{
	CalendarOutlookEvent Model;
    public OutLookDetailsPopup(CalendarOutlookEvent model)
	{
		InitializeComponent();
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

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void JoinNow_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Uri uri = new Uri(Model.OnlineMeeting.JoinUrl!);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }
}
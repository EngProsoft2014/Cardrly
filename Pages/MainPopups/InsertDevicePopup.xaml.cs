using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using Mopups.Services;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Cardrly.Pages.MainPopups;

public partial class InsertDevicePopup : Mopups.Pages.PopupPage
{
    public delegate void DeviceDelegte(string Uri);
    public event DeviceDelegte DeviceClose;

    public InsertDevicePopup()
	{
		InitializeComponent();

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

    private async void Save_Clicked(object sender, EventArgs e)
    {
        // check value is a url
        string valid = "";
        if (string.IsNullOrEmpty(ValueEn.Text))
        {
            var toast = Toast.Make($"{AppResources.msgFRLink}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
            return;
        }
        else
        {
            valid = CheckStringType(ValueEn.Text);
        }
        // return data 
        if (valid != "URL")
        {
            var toast = Toast.Make($"{AppResources.msgThevaluenotmatchlinkformat}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
        else
        {
            DeviceClose.Invoke(ValueEn.Text);
            await MopupService.Instance.PopAsync(true);
        }
    }



    private void Cancel_Clicked(object sender, EventArgs e)
    {
        MopupService.Instance.PopAsync();
    }

    public string CheckStringType(string input)
    {

        // URL pattern
        string urlPattern = @"^(http|https)://[^\s/$.?#].[^\s]*$";


        if (Regex.IsMatch(input, urlPattern))
        {
            return "URL";
        }
        else
        {
            return "Unknown";
        }
    }
}
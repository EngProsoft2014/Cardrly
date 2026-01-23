using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using Mopups.Services;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Cardrly.Pages.MainPopups;

public partial class InsertDevicePopup : Mopups.Pages.PopupPage
{
    public delegate void DeviceDelegte(string Uri, string UrlRedirect);
    public event DeviceDelegte DeviceClose;

    public InsertDevicePopup(bool isShowUrl)
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

        stkURL.IsVisible = isShowUrl;
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        // check value is a url
        string valid = "";
        string validRedirect = "";
        if ((stkURL.IsVisible == true && string.IsNullOrEmpty(ValueEn.Text)) || string.IsNullOrEmpty(ValueEnRedirect.Text))
        {
            var toast = Toast.Make($"{AppResources.msgFRLink}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
            return;
        }
        else
        {
            if(!string.IsNullOrEmpty(ValueEn.Text))
                valid = CheckStringGuidType(ValueEn.Text);

            validRedirect = CheckStringType(ValueEnRedirect.Text);
        }
        // return data 
        if ((!string.IsNullOrEmpty(ValueEn.Text) && valid != "URLGuid") || validRedirect != "URL")
        {
            var toast = Toast.Make($"{AppResources.msgThevaluenotmatchlinkformat}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
        else
        {
            DeviceClose.Invoke(ValueEn.Text == null ? string.Empty : ValueEn.Text, ValueEnRedirect.Text);
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

    public string CheckStringGuidType(string input)
    {
        string pattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

        if (Regex.IsMatch(input, pattern))
        {
            return "URLGuid";
        }
        else
        {
            return "Unknown";
        }
    }
}
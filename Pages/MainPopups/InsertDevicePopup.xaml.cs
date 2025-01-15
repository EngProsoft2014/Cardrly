using Android.Bluetooth;
using Cardrly.Helpers;
using CommunityToolkit.Maui.Alerts;
using Mopups.Services;
using System.Text.RegularExpressions;

namespace Cardrly.Pages.MainPopups;

public partial class InsertDevicePopup : Mopups.Pages.PopupPage
{
    public delegate void DeviceDelegte(string Uri);
    public event DeviceDelegte DeviceClose;

    public InsertDevicePopup()
	{
		InitializeComponent();
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        // check value is a url
        string valid = "";
        if (string.IsNullOrEmpty(ValueEn.Text))
        {
            var toast = Toast.Make("Require Field : Link", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
            var toast = Toast.Make("The value not match link format", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
        else
        {
            DeviceClose.Invoke(ValueEn.Text);
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
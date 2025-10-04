using Cardrly.Models;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using System.Globalization;

namespace Cardrly.Pages.MainPopups;

public partial class UpdateVersionPopup : Mopups.Pages.PopupPage
{
    UpdateVersionModel _obj;

    public UpdateVersionPopup(UpdateVersionModel obj)
	{
		InitializeComponent();

        _obj = obj;

        string LangValueToKeep = Preferences.Default.Get("Lan", "en");

        lblMsg.Text = LangValueToKeep == "ar" ? obj.DescriptionAr : obj.Description;

        btnStoreLink.Text = obj.Name + " | " + $"Version {obj.VersionNumber} ({obj.VersionBuild})";

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


    [Obsolete]
    protected override bool OnBackButtonPressed()
    {
        // Return true to prevent the default behavior
        return true;
    }

    private async void btnStoreLink_Clicked(object sender, EventArgs e)
    {
        this.IsEnabled = false;
        if(DeviceInfo.Platform == DevicePlatform.Android && _obj.Name.ToLower() == "android")
        {
            try
            {
                Uri uri = new Uri("https://play.google.com/store/apps/details?id=com.companyname.cardrly&hl=en");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        else if(DeviceInfo.Platform == DevicePlatform.iOS && _obj.Name.ToLower() == "ios")
        {
            try
            {
                Uri uri = new Uri("https://apps.apple.com/us/app/cardrly/id6739498351");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        this.IsEnabled = true;
    }
}
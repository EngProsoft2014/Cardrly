using Cardrly.Extensions;
using Cardrly.Helpers;
using Cardrly.ViewModels;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Globalization;

namespace Cardrly.Pages.MainPopups;

public partial class LanguagePopup : Mopups.Pages.PopupPage
{
    IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    public LanguagePopup(IGenericRepository generic, Services.Data.ServicesService service)
	{
        InitializeComponent();
        Rep = generic;
        _service = service;
        LoadSetting();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
		await MopupService.Instance.PopAsync();
    }


    //Arabic
    private async void ArabicTap(object sender, TappedEventArgs e)
    {
        CultureInfo cal = new CultureInfo("ar");
        TranslateExtension.Instance.SetCulture(cal);
        Preferences.Default.Set("Lan", "ar");
        LoadSetting();
        await MopupService.Instance.PopAsync();
        App.Current!.MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service, Controls.StaticMember._audioManager), Rep, _service));
    }

    //English
    private async void EnglishTap(object sender, TappedEventArgs e)
    {
        CultureInfo cal = new CultureInfo("en");
        TranslateExtension.Instance.SetCulture(cal);
        Preferences.Default.Set("Lan", "en");
        LoadSetting();
        await MopupService.Instance.PopAsync();
        App.Current!.MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service, Controls.StaticMember._audioManager), Rep, _service));
    }


    void LoadSetting()
    {
        string Lan = Preferences.Default.Get("Lan", "en");
        Color color = Color.FromHex("#FF7F3E");
        if (Lan == "ar")
        {
            lblArabic.TextColor = color;

            lblEnglish.TextColor = Color.FromHex("#333");

            Task.Delay(1000);
        } 
        else
        {
            lblEnglish.TextColor = color;

            lblArabic.TextColor = Color.FromHex("#333");

            Task.Delay(1000);
        }
    }
}
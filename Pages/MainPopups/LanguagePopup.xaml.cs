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
        UserDialogs.Instance.ShowLoading();
        CultureInfo cal = new CultureInfo("ar");
        TranslateExtension.Instance.SetCulture(cal);
        Preferences.Default.Set("Lan", "ar");

        imgCheckArabic.IsVisible = true;
        imgCheckEnglish.IsVisible = false;

        LoadSetting();
        await MopupService.Instance.PopAsync();
        App.Current!.MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service, Controls.StaticMember._audioManager), Rep, _service));
        UserDialogs.Instance.HideHud();
    }

    //English
    private async void EnglishTap(object sender, TappedEventArgs e)
    {
        UserDialogs.Instance.ShowLoading();

        CultureInfo cal = new CultureInfo("en");
        TranslateExtension.Instance.SetCulture(cal);
        Preferences.Default.Set("Lan", "en");
        imgCheckEnglish.IsVisible = true;
        imgCheckArabic.IsVisible = false;
        LoadSetting();
        await MopupService.Instance.PopAsync();
        App.Current!.MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service, Controls.StaticMember._audioManager), Rep, _service));

        UserDialogs.Instance.HideHud();
    }


    void LoadSetting()
    {
        string Lan = Preferences.Default.Get("Lan", "en");
        Color color = Color.FromHex("#FF7F3E");
        if (Lan == "ar")
        {
            lblArabic.TextColor = color;

            imgCheckArabic.IsVisible = true;
            imgCheckEnglish.IsVisible = false;

            lblEnglish.TextColor = Color.FromHex("#333");

            Task.Delay(1000);
        } 
        else
        {
            lblEnglish.TextColor = color;

            imgCheckEnglish.IsVisible = true;
            imgCheckArabic.IsVisible = false;

            lblArabic.TextColor = Color.FromHex("#333");

            Task.Delay(1000);
        }
    }
}
using Cardrly.Extensions;
using Cardrly.Helpers;
using Cardrly.Services.AudioStream;
using Cardrly.Services.Data;
using Cardrly.ViewModels;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Globalization;

namespace Cardrly.Pages.MainPopups;

public partial class LanguagePopup : Mopups.Pages.PopupPage
{
    IGenericRepository Rep;
    readonly ServicesService _service;
    readonly SignalRService _signalRService;
    readonly IAudioStreamService _audioService;
    readonly LocationTrackingService _locationTracking;
    public LanguagePopup(IGenericRepository generic, ServicesService service, SignalRService signalRService, IAudioStreamService audioService, LocationTrackingService locationTracking)
	{
        InitializeComponent();
        Rep = generic;
        _service = service;
        _signalRService = signalRService;
        _audioService = audioService;
        _locationTracking = locationTracking;
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
        SetUIMethod(cal);
        LoadSetting();
        await MopupService.Instance.PopAsync();
        Reset();
    }

    //English
    private async void EnglishTap(object sender, TappedEventArgs e)
    {
        CultureInfo cal = new CultureInfo("en");
        TranslateExtension.Instance.SetCulture(cal);
        Preferences.Default.Set("Lan", "en");
        SetUIMethod(cal);
        LoadSetting();
        await MopupService.Instance.PopAsync();
        Reset();
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

    void Reset()
    {
        (Application.Current as App).MainPage.Dispatcher.Dispatch(() =>
        {
            App.Current!.MainPage = new NavigationPage(new HomePage(new HomeViewModel(Rep, _service, _signalRService, _audioService, _locationTracking), Rep, _service, _signalRService, _audioService, _locationTracking));
        });
    }

    public void SetUIMethod(CultureInfo culture)
    {
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;


        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // Update UI FlowDirection (if applicable)
        var isRtl = culture.TextInfo.IsRightToLeft;
        Application.Current.MainPage.FlowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }
}
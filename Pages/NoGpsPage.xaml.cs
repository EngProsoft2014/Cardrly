using Cardrly.Helpers;
using Cardrly.Services.AudioStream;
using Cardrly.Services.Data;
using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class NoGpsPage : Controls.CustomControl
{
    #region Service
    readonly IGenericRepository Rep;
    readonly ServicesService _service;
    readonly SignalRService _signalRService;
    readonly IAudioStreamService _audioService;
    readonly LocationTrackingService _locationTracking;
    #endregion

    public NoGpsPage(IGenericRepository GenericRep, ServicesService service, SignalRService signalRService, IAudioStreamService audioService, LocationTrackingService locationTracking)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        _signalRService = signalRService;
        _audioService = audioService;
        _locationTracking = locationTracking;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CheckGpsStatus();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await CheckGpsStatus();
    }

    private async Task CheckGpsStatus()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(1));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                // GPS is enabled → go back to HomePage
                if (App.Current!.MainPage!.Navigation.NavigationStack.Count > 1)
                    await App.Current.MainPage.Navigation.PopAsync();
                else
                    await App.Current!.MainPage!.Navigation.PushAsync(new HomePage(new HomeViewModel(Rep, _service, _signalRService, _audioService, _locationTracking), Rep, _service, _signalRService, _audioService, _locationTracking));
            }
        }
        catch (FeatureNotEnabledException)
        {
            // Still disabled → stay on this page
        }
        catch (PermissionException)
        {
            // Permission denied → stay on this page
        }
    }
}
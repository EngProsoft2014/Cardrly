using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Resources.Lan;
using Cardrly.Services;
using Cardrly.Services.AudioStream;
using Cardrly.Services.Data;
using Cardrly.ViewModels;
using Controls.UserDialogs.Maui;

namespace Cardrly.Pages;

public partial class NoInternetPage : Controls.CustomControl
{

    #region Service
    readonly IGenericRepository Rep;
    readonly ServicesService _service;
    readonly SignalRService _signalRService;
    readonly IAudioStreamService _audioService;
    readonly LocationTrackingService _locationTracking;
    private static CancellationTokenSource _notifyCts;
    #endregion

    public NoInternetPage(IGenericRepository GenericRep, ServicesService service, SignalRService signalRService, IAudioStreamService audioService, LocationTrackingService locationTracking)
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

        Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
    }


    protected override bool OnBackButtonPressed()
    {
        // Run the async code on the UI thread
        Dispatcher.Dispatch(() =>
        {
            Action action = () => Application.Current!.Quit();
            Controls.StaticMember.ShowSnackBar(AppResources.msgDoYouWantToLogout, Controls.StaticMember.SnackBarColor, Controls.StaticMember.SnackBarTextColor, action);
        });

        // Return true to prevent the default behavior
        return true;
    }

    async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            // Connection to internet is Not available
        }
        else
        {
            // Connection to internet is available
            await GoAfterConnected();
        }
    }

    public async Task GoAfterConnected()
    {
        UserDialogs.Instance.ShowLoading();
        //await App.Current.MainPage.Navigation.PushAsync(Name);
        if (App.Current!.MainPage!.Navigation.NavigationStack.Count > 1)
        {
            await App.Current!.MainPage!.Navigation.PopAsync();
        }
        else
        {
            string userId = Preferences.Default.Get(ApiConstants.userid, string.Empty);
            if (string.IsNullOrEmpty(userId))
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(Rep, _service, _signalRService, _audioService, _locationTracking)));
            }
            else
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new HomePage(new HomeViewModel(Rep, _service, _signalRService, _audioService, _locationTracking), Rep, _service, _signalRService, _audioService, _locationTracking));
            }
        }

        //App.Current.MainPage.Navigation.RemovePage(App.Current.MainPage.Navigation.NavigationStack[App.Current.MainPage.Navigation.NavigationStack.Count - 2]);

        UserDialogs.Instance.HideHud();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            // Connection to internet is Not available

        }
        else
        {
            // Connection to internet is available
            await GoAfterConnected();
        }
    }
}
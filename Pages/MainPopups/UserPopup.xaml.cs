using Mopups.Services;
using Cardrly.Helpers;
using Cardrly.ViewModels;

namespace Cardrly.Pages.MainPopups;

public partial class UserPopup : Mopups.Pages.PopupPage
{
    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public UserPopup(IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
		await MopupService.Instance.PopAsync();
    }

    private async void TapGestureRecognizer_Custom_Vcard(object sender, TappedEventArgs e)
    {
        var vm = new AddCustomCardViewModel(Rep,_service);
        var page = new AddCustomCardPage(vm);
        page.BindingContext = vm;
        await App.Current!.MainPage!.Navigation.PushAsync(page);
        await MopupService.Instance.PopAsync();
    }

    private void TapGestureRecognizer_CreateLink(object sender, TappedEventArgs e)
    {

    }
}
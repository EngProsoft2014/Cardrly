using Mopups.Services;

namespace Cardrly.Pages.MainPopups;

public partial class ReadyToScanPopup : Mopups.Pages.PopupPage
{
	public ReadyToScanPopup()
	{
		InitializeComponent();
	}


    private async void Cancel_Clicked(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }
}
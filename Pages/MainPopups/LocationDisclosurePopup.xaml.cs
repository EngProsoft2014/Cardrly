using Mopups.Pages;
using Mopups.Services;

namespace Cardrly.Pages.MainPopups;

public partial class LocationDisclosurePopup : PopupPage
{
    private readonly TaskCompletionSource<bool> _tcs = new();

    public LocationDisclosurePopup()
	{
		InitializeComponent();
	}

    protected override bool OnBackButtonPressed()
    {
        // ≈—Ã«⁄ true Ì⁄‰Ì „‰⁄ ≈€·«ﬁ «·‹ Popup »“— «·—ÃÊ⁄
        return true;
    }

    private void OnAllowClicked(object sender, EventArgs e)
    {
        Preferences.Default.Set(Constants.ApiConstants.isLocationDisclosureAccepted, true);
        _tcs.TrySetResult(true);
        MopupService.Instance.PopAsync();
    }

    private void OnDenyClicked(object sender, EventArgs e)
    {
        Preferences.Default.Set(Constants.ApiConstants.isLocationDisclosureAccepted, false);
        _tcs.TrySetResult(false);
        MopupService.Instance.PopAsync();
    }

    public Task<bool> ShowAsync()
    {
        return _tcs.Task;
    }
}
using Cardrly.Resources.Lan;

namespace Cardrly.Pages.MainPopups;

public partial class UpdateVersionPopup : Mopups.Pages.PopupPage
{
	public UpdateVersionPopup()
	{
		InitializeComponent();
	}

    [Obsolete]
    protected override bool OnBackButtonPressed()
    {
        // Return true to prevent the default behavior
        return true;
    }
}
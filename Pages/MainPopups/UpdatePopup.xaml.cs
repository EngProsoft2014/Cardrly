using Cardrly.Resources.Lan;

namespace Cardrly.Pages.MainPopups;

public partial class UpdatePopup : Mopups.Pages.PopupPage
{
	public UpdatePopup()
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
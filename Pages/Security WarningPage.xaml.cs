using Cardrly.Resources.Lan;

namespace Cardrly.Pages;

public partial class Security_WarningPage : Controls.CustomControl
{
	public Security_WarningPage(string msg)
	{
		InitializeComponent();
        lblWar.Text = msg;
	}

    protected override bool OnBackButtonPressed()
    {
        // Run the async code on the UI thread
        Dispatcher.Dispatch(() =>
        {
            Action action = () => Application.Current!.Quit();
            Controls.StaticMember.ShowSnackBar($"{AppResources.msgDoYouWantToLogout}", Controls.StaticMember.SnackBarColor, Controls.StaticMember.SnackBarTextColor, action);
        });

        // Return true to prevent the default behavior
        return true;
    }
}
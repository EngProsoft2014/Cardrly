using Cardrly.Resources.Lan;
using Cardrly.ViewModels;


namespace Cardrly.Pages;

public partial class LoginPage : Controls.CustomControl
{
    LoginViewModel Model;

    public LoginPage(LoginViewModel model)
	{
		InitializeComponent();
        this.BindingContext = model;
        Model = model;

        entryEmail.Completed += (object sender, EventArgs e) =>
        {
            entryPassword.Focus();
        };
        entryPassword.Completed += (object sender, EventArgs e) =>
        {
            Model.LoginClickCommand.Execute(Model.LoginRequest);
        };
    }

    [Obsolete]
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

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        entryPassword.IsPassword = (entryPassword.IsPassword == true) ? false : true;
    }
}
using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class ChangePasswordPage : Controls.CustomControl
{
	public ChangePasswordPage(ChangePasswordViewModel model)
	{
		InitializeComponent();
		this.BindingContext = model;
    }
}
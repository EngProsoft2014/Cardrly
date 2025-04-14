using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class ResetPasswordPage : Controls.CustomControl
{
	public ResetPasswordPage(ResetPasswordViewModel model)
	{
		InitializeComponent();
		this.BindingContext = model;
	}
}
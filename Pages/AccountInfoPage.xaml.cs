using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class AccountInfoPage : Controls.CustomControl
{
	public AccountInfoPage(AccountInfoViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;	
	}
}
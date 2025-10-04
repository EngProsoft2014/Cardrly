using Cardrly.ViewModels.Links;

namespace Cardrly.Pages.Links;

public partial class AddLinksPage : Controls.CustomControl
{
	public AddLinksPage(AddLinkViewModel model)
	{
		InitializeComponent();
		this.BindingContext = model;
	}
}
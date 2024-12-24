using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class AllCommentPage : Controls.CustomControl
{
	AllCommentViewModel Model;
	public AllCommentPage(AllCommentViewModel model)
	{
		InitializeComponent();
		this.BindingContext = model;
	}
}
using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class AddEventPage : Controls.CustomControl
{
	public AddEventPage(AddEventViewModel model)
	{
		InitializeComponent();
        this.BindingContext = model;
    }
}
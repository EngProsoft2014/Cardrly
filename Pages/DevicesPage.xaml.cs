using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class DevicesPage : Controls.CustomControl
{
	public DevicesPage(DevicesViewModel model)
	{
		InitializeComponent();
		this.BindingContext = model;
    }
}
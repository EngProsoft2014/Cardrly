using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class TimeZonePage : Controls.CustomControl
{
	public TimeZonePage(TimeZoneViewModel model)
	{
		InitializeComponent();
		this.BindingContext = model;
    }
}
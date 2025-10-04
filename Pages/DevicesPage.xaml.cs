using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class DevicesPage : Controls.CustomControl
{
    DevicesViewModel Model;

    public DevicesPage(DevicesViewModel model)
	{
		InitializeComponent();
		this.BindingContext = Model = model;
    }

    private async void RefView_Refreshing(object sender, EventArgs e)
    {
        RefView.IsRefreshing = true;
        await Model.GetAllDevices();
        RefView.IsRefreshing = false;
    }
}
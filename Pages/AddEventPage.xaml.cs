using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class AddEventPage : Controls.CustomControl
{
	public AddEventPage(AddEventViewModel model)
	{
        InitializeComponent();
        this.BindingContext = model;
    }

    //private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    PlaceholderProviderLabel.IsVisible = false;
    //}

    private void picCard_SelectedIndexChanged(object sender, EventArgs e)
    {
        PlaceholderCardLabel.IsVisible = false;
    }
}
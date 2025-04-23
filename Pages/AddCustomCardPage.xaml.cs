using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class AddCustomCardPage : Controls.CustomControl
{
	AddCustomCardViewModel Model;
	public AddCustomCardPage(AddCustomCardViewModel model)
	{
		InitializeComponent();
        this.BindingContext = Model = model;
	}

    private void LinkColor_PickedColorChanged(object sender, Maui.ColorPicker.PickedColorChangedEventArgs e)
    {
        if (e.NewPickedColorValue != null)
        {
            Model.Request.LinkColor = e.NewPickedColorValue.ToHex();
        }
    }

    private void CardName_PickedColorChanged(object sender, Maui.ColorPicker.PickedColorChangedEventArgs e)
    {
        if (e.NewPickedColorValue != null)
        {
            Model.Request.CardTheme = e.NewPickedColorValue.ToHex();
        }
    }
}
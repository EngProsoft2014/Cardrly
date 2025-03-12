using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class AddEventPage : Controls.CustomControl
{
	public AddEventPage(AddEventViewModel model)
	{
		InitializeComponent();
        this.BindingContext = model;
        Init();
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        PlaceholderProviderLabel.IsVisible = false;
    }

    private void picCard_SelectedIndexChanged(object sender, EventArgs e)
    {
        PlaceholderCardLabel.IsVisible = false;
    }

    void Init()
    {
        string Lan = Preferences.Default.Get("Lan", "en");
        if (Lan == "ar")
        {
            this.FlowDirection = FlowDirection.RightToLeft;
            StartDatePicker.FlowDirection = FlowDirection.RightToLeft;
            EndDatePicker.FlowDirection = FlowDirection.RightToLeft;
            StartTimePicker.FlowDirection = FlowDirection.RightToLeft;
            EndTimePicker.FlowDirection = FlowDirection.RightToLeft;
        }
        else
        {
            this.FlowDirection = FlowDirection.LeftToRight;
            StartDatePicker.FlowDirection = FlowDirection.LeftToRight;
            EndDatePicker.FlowDirection = FlowDirection.LeftToRight;
            StartTimePicker.FlowDirection = FlowDirection.LeftToRight;
            EndTimePicker.FlowDirection = FlowDirection.LeftToRight;
        }
    }
}
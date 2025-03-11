using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class TimeZonePage : Controls.CustomControl
{
    TimeZoneViewModel Model;
    public TimeZonePage(TimeZoneViewModel model)
	{
		InitializeComponent();
		this.BindingContext = model;
        Model = model;
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        Calc.ItemsSource = Model.AllTimeZones.Where(x => x.ToLower().Contains(e.NewTextValue.ToLower()));
    }
}
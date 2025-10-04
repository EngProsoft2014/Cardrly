using Cardrly.ViewModels;
using System.Collections.ObjectModel;

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
        if (string.IsNullOrEmpty(e.NewTextValue))
        {
            Model.ViewLst = new ObservableCollection<string>(Model.AllTimeZones.Take(20).ToList());
            Model.NumberOfPage = 1;
        }
        else
        {
            Calc.ItemsSource = Model.AllTimeZones.Where(x => x.ToLower().Contains(e.NewTextValue.ToLower()));
        }
        
    }
}
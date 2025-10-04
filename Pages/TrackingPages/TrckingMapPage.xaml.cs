using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Cardrly.Pages.TrackingPages;

public partial class TrckingMapPage : Controls.CustomControl
{
    #region Service
    readonly IGenericRepository ORep;
    readonly Services.Data.ServicesService _service;
    #endregion
    EmployeesViewModel employeesViewModel;

    public TrckingMapPage(EmployeesViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        ORep = GenericRep;
        _service = service;
        this.BindingContext = employeesViewModel = model;
    }

    public TrckingMapPage(DataMapsModel? dataMap, EmployeesViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();

        ORep = GenericRep;
        _service = service;
        this.BindingContext = employeesViewModel = model;

        if (dataMap != null)
        {
            Pin pin = new Pin
            {
                Label = dataMap.Id.ToString(),
                //Address = "The city with a boardwalk",
                Type = PinType.Place,
                Location = new Location(dataMap.MPosition)
            };
            myMap.Pins.Add(pin);

            myMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                   new Location(double.Parse(dataMap.Lat), double.Parse(dataMap.Long)), Distance.FromMeters(200)));
        }
       
    }

    private void myMap_MapClicked(object sender, MapClickedEventArgs e)
    {
        myMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(double.Parse(employeesViewModel.LastListmap.LastOrDefault().Lat), double.Parse(employeesViewModel.LastListmap.LastOrDefault().Long)), Distance.FromMeters(100)));
    }



}
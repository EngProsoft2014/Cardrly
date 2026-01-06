using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Services.Data;
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

        model._signalRLocation = DependencyService.Resolve<SignalRService>();

        model._signalRLocation.OnMessageReceivedLocation += (locationData) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateLocationOnMap(locationData);
            });
        };
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        employeesViewModel._signalRLocation.StopLocationTracking();
    }

    private void myMap_MapClicked(object sender, MapClickedEventArgs e)
    {
        myMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(double.Parse(employeesViewModel.LastListmap.LastOrDefault().Lat), double.Parse(employeesViewModel.LastListmap.LastOrDefault().Long)), Distance.FromMeters(100)));
    }

    public void UpdateLocationOnMap(DataMapsModel dataMap)
    {
        if (dataMap == null) return;

        // Clear old pins if you only want one
        myMap.Pins.Clear();

        // Add new pin
        var pin = new Pin
        {
            Label = dataMap.Id.ToString(),
            Type = PinType.Place,
            Location = new Location(double.Parse(dataMap.Lat), double.Parse(dataMap.Long))
        };
        myMap.Pins.Add(pin);

        // Move map to new location
        myMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(double.Parse(dataMap.Lat), double.Parse(dataMap.Long)),
            Distance.FromMeters(200)));
    }

    //public TrckingMapPage(EmployeesViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
    //{
    //    InitializeComponent();

    //    ORep = GenericRep;
    //    _service = service;
    //    this.BindingContext = employeesViewModel = model;




    //    //if (dataMap != null)
    //    //{
    //    //    Pin pin = new Pin
    //    //    {
    //    //        Label = dataMap.Id.ToString(),
    //    //        //Address = "The city with a boardwalk",
    //    //        Type = PinType.Place,
    //    //        Location = new Location(dataMap.MPosition)
    //    //    };
    //    //    myMap.Pins.Add(pin);

    //    //    myMap.MoveToRegion(MapSpan.FromCenterAndRadius(
    //    //           new Location(double.Parse(dataMap.Lat), double.Parse(dataMap.Long)), Distance.FromMeters(200)));
    //    //}
       
    //}

}
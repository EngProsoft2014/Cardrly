using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Services.Data;
using Cardrly.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Threading.Tasks;

namespace Cardrly.Pages.TrackingPages;

public partial class TrckingMapPage : Controls.CustomControl
{
    #region Service
    readonly IGenericRepository ORep;
    readonly Services.Data.ServicesService _service;
    readonly SignalRService _signalRService;
    #endregion
    EmployeesViewModel employeesViewModel;

    public TrckingMapPage(EmployeesViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service, SignalRService signalRService)
	{
		InitializeComponent();
        ORep = GenericRep;
        _service = service;
        _signalRService = signalRService;
        this.BindingContext = employeesViewModel = model;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _signalRService.OnMessageReceivedLocation += HandleLocationUpdate;

        Task.WhenAll(LastLocationEmployee());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _signalRService.OnMessageReceivedLocation -= HandleLocationUpdate;
    }

    private void HandleLocationUpdate(DataMapsModel locationData)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLocationOnMap(locationData);
        });
    }


    private void myMap_MapClicked(object sender, MapClickedEventArgs e)
    {
        myMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(employeesViewModel.LastListmap.LastOrDefault().Lat, employeesViewModel.LastListmap.LastOrDefault().Long), Distance.FromMeters(100)));
    }

    async Task LastLocationEmployee()
    {
        await employeesViewModel.GetLastLocation(employeesViewModel.OneEmployee.Id);

        if (employeesViewModel.LastLocation != null)
        {
            var pin = new Pin
            {
                Label = "Time " + employeesViewModel.LastLocation.Time.ToString(@"hh\:mm\:ss"),
                Type = PinType.Place,
                Location = new Location(employeesViewModel.LastLocation.Latitude, employeesViewModel.LastLocation.Longitude)
            };
            myMap.Pins.Add(pin);

            // Move map to new location
            myMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(employeesViewModel.LastLocation.Latitude, employeesViewModel.LastLocation.Longitude),
                Distance.FromMeters(200)));
        }
    }

    public void UpdateLocationOnMap(DataMapsModel dataMap)
    {
        if (dataMap == null) return;

        // Clear old pins if you only want one
        myMap.Pins.Clear();

        // Add new pin
        var pin = new Pin
        {
            Label = "Time " + dataMap.Time.ToString(@"hh\:mm\:ss"),
            Type = PinType.Place,
            Location = new Location(dataMap.Lat, dataMap.Long)
        };
        myMap.Pins.Add(pin);

        // Move map to new location
        myMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(dataMap.Lat, dataMap.Long),
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
using Cardrly.Models.TimeSheet;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;


namespace Cardrly.Pages.TrackingPages;

public partial class HistoryTrackingMapPage : Controls.CustomControl
{
    CancellationTokenSource _cts;
    List<EmployeeLocationResponse> _locations;
    Pin _movingPin;

    public HistoryTrackingMapPage(List<EmployeeLocationResponse> locations)
	{
		InitializeComponent();

        // Order by time
        var ordered = locations.OrderBy(l => l.CreateDate).ToList();

        // Draw pins
        foreach (var loc in ordered)
        {
            EmployeeMap.Pins.Add(new Pin
            {
                Label = $"Point at {loc.CreateDate}",
                Location = new Location(loc.Latitude, loc.Longitude),
                Type = PinType.Place
            });
        }

        // Center map on first point
        if (ordered.Any())
        {
            EmployeeMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(ordered.First().Latitude, ordered.First().Longitude),
                Distance.FromKilometers(1)));
        }
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        _cts = new CancellationTokenSource();

        if (_movingPin == null)
        {
            _movingPin = new Pin { Label = "Employee", Type = PinType.Generic };
            EmployeeMap.Pins.Add(_movingPin);
        }

        var ordered = _locations.OrderBy(l => l.CreateDate).ToList();
        int total = ordered.Count;

        for (int i = 0; i < total; i++)
        {
            if (_cts.IsCancellationRequested) break;

            var loc = ordered[i];
            _movingPin.Location = new Location(loc.Latitude, loc.Longitude);

            ProgressSlider.Value = (double)i / total * 100;

            await Task.Delay(1000); // 1 sec per point, adjust speed
        }
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    private void OnProgressChanged(object sender, ValueChangedEventArgs e)
    {
        var ordered = _locations.OrderBy(l => l.CreateDate).ToList();
        int index = (int)(ordered.Count * (e.NewValue / 100));
        if (index >= 0 && index < ordered.Count)
        {
            var loc = ordered[index];
            if (_movingPin != null)
                _movingPin.Location = new Location(loc.Latitude, loc.Longitude);
        }
    }

}
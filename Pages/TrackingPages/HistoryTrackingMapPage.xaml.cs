using Cardrly.Models.TimeSheet;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Threading.Tasks;


namespace Cardrly.Pages.TrackingPages;

public partial class HistoryTrackingMapPage : Controls.CustomControl
{
    CancellationTokenSource _cts;
    List<EmployeeLocationResponse> _locations;
    private bool _isPlaying = false;
    private int _currentIndex = 0;
    Pin _movingPin;

    public HistoryTrackingMapPage(List<EmployeeLocationResponse> locations, string CardName)
    {
        InitializeComponent();

        titlePage.Text = CardName;

        _locations = locations;

        // Order by time
        var ordered = locations.OrderBy(l => l.CreateDate).ToList();

        // Center map on first point
        if (ordered.Any())
        {
            // Draw first pin
            _movingPin = new Pin
            {
                Label = $"Point at {ordered?.FirstOrDefault()?.CreateDate}",
                Location = new Location(ordered.FirstOrDefault().Latitude, ordered.FirstOrDefault().Longitude),
                Type = PinType.Place
            };
            EmployeeMap.Pins.Add(_movingPin);

            EmployeeMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(ordered.First().Latitude, ordered.First().Longitude),
                Distance.FromKilometers(1)));
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var ordered = _locations.OrderBy(l => l.CreateDate).ToList();
        if (!ordered.Any())
            return;

        if (_isPlaying)
        {
            // Pause
            PlayPauseImage.Source = new FontImageSource
            {
                FontFamily = "FontIconSolid",
                Glyph = "\uf04b", // Play
                Color = Colors.OrangeRed,
                Size = 30
            };
            PlayPauseImage.Margin = 0;
            _isPlaying = false;
            _cts?.Cancel();
        }
        else
        {
            // Play
            _cts = new CancellationTokenSource();
            PlayPauseImage.Source = new FontImageSource
            {
                FontFamily = "FontIconSolid",
                Glyph = "\uf04c", // Pause
                Color = Colors.OrangeRed,
                Size = 32, 
            };
            PlayPauseImage.Margin = new Thickness(5, 0, 0, 0);
            _isPlaying = true;

            if (_movingPin == null)
            {
                _movingPin = new Pin
                {
                    Label = ordered[_currentIndex].Time.ToString("HH:mm:ss"),
                    Type = PinType.Place,
                    Location = new Location(ordered[_currentIndex].Latitude, ordered[_currentIndex].Longitude)
                };
                EmployeeMap.Pins.Add(_movingPin);
            }

            int total = ordered.Count;

            for (int i = _currentIndex; i < total; i++)
            {
                if (_cts.IsCancellationRequested)
                {
                    _currentIndex = i; // remember where we paused
                    break;
                }

                var loc = ordered[i];


                if (EmployeeMap.Pins.Contains(_movingPin))
                    EmployeeMap.Pins.Remove(_movingPin);

                _movingPin = new Pin
                {
                    Label = ordered[_currentIndex].Time.ToString("HH:mm:ss"),
                    Type = PinType.Place,
                    Location = new Location(loc.Latitude, loc.Longitude)
                };

                EmployeeMap.Pins.Add(_movingPin);

                ProgressSlider.Value = (double)i / total * 100;
                TimeLabel.Text = loc.CreateDate.ToLocalTime().ToString("HH:mm:ss");

                _currentIndex = i;   // 👈 update here every step

                await Task.Delay(1000);
            }

            // Reset when finished
            if (_currentIndex >= total - 1)
            {
                PlayPauseImage.Source = new FontImageSource
                {
                    FontFamily = "FontIconSolid",
                    Glyph = "\uf04b", // Play
                    Color = Colors.OrangeRed,
                    Size = 30
                };
                PlayPauseImage.Margin = 0;
                _isPlaying = false;
                _currentIndex = 0;
            }
        }
    }


    private void OnProgressChanged(object sender, ValueChangedEventArgs e)
    {
        var ordered = _locations.OrderBy(l => l.CreateDate).ToList();
        if (!ordered.Any())
            return;

        int total = ordered.Count;
        int index = (int)(total * (e.NewValue / 100));

        if (index >= 0 && index < total)
        {
            var loc = ordered[index];

            // Move pin immediately
            if (_movingPin != null)
            {
                _movingPin.Location = new Location(loc.Latitude, loc.Longitude);
            }

            // Show time in label
            TimeLabel.Text = loc.CreateDate.ToLocalTime().ToString("HH:mm:ss");

            // Update playback position
            _currentIndex = index;
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

   
}
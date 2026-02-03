
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
                Distance.FromMeters(200)));
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

            // Ensure pin exists
            if (_movingPin == null)
            {
                var loc = ordered[_currentIndex];
                _movingPin = new Pin
                {
                    Label = loc.Time.ToString(@"hh\:mm\:ss"),
                    Type = PinType.Place,
                    Location = new Location(loc.Latitude, loc.Longitude)
                };
                EmployeeMap.Pins.Add(_movingPin);
            }

            // Always delegate playback to helper
            _ = ContinuePlaybackFromIndex(_currentIndex, ordered, _cts.Token);
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
            if (_movingPin != null && EmployeeMap.Pins.Contains(_movingPin))
                EmployeeMap.Pins.Remove(_movingPin);

            _movingPin = new Pin
            {
                Label = loc.Time.ToString(@"hh\:mm\:ss"),
                Type = PinType.Place,
                Location = new Location(loc.Latitude, loc.Longitude)
            };
            EmployeeMap.Pins.Add(_movingPin);

            // Show time in label
            TimeLabel.Text = loc.CreateDate.TimeOfDay.ToString(@"hh\:mm\:ss");

            // Update playback position
            _currentIndex = index;

            // Move map immediately
            EmployeeMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(loc.Latitude, loc.Longitude),
                Distance.FromMeters(200)));

            // If playing, restart playback from new index
            if (_isPlaying)
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                _ = ContinuePlaybackFromIndex(_currentIndex, ordered, _cts.Token);
            }
        }
    }

    private async Task ContinuePlaybackFromIndex(int startIndex, List<EmployeeLocationResponse> ordered, CancellationToken token)
    {
        int total = ordered.Count;
        for (int i = startIndex; i < total; i++)
        {
            if (token.IsCancellationRequested)
            {
                _currentIndex = i;
                break;
            }

            var loc = ordered[i];

            if (EmployeeMap.Pins.Contains(_movingPin))
                EmployeeMap.Pins.Remove(_movingPin);

            _movingPin = new Pin
            {
                Label = loc.Time.ToString(@"hh\:mm\:ss"),
                Type = PinType.Place,
                Location = new Location(loc.Latitude, loc.Longitude)
            };
            EmployeeMap.Pins.Add(_movingPin);

            ProgressSlider.Value = (double)i / total * 100;
            TimeLabel.Text = loc.CreateDate.TimeOfDay.ToString(@"hh\:mm\:ss");
            _currentIndex = i;

            EmployeeMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(loc.Latitude, loc.Longitude),
                Distance.FromMeters(200)));

            await Task.Delay(1000);
        }

        // Reset when finished
        if (_currentIndex >= total - 1)
        {
            _isPlaying = false;
            _currentIndex = 0;
            PlayPauseImage.Source = new FontImageSource
            {
                FontFamily = "FontIconSolid",
                Glyph = "\uf04b", // Play
                Color = Colors.OrangeRed,
                Size = 30
            };
            PlayPauseImage.Margin = 0;
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

   
}
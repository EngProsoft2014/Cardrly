using System.ComponentModel;
namespace Cardrly.Models.Calendar
{
    public class CalendarGmailRequest : INotifyPropertyChanged
    {
        public string? Summary { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? Attendees { get; set; }
        public DateTime Start { get; set; } = DateTime.UtcNow.Date;
        public DateTime End { get; set; } = DateTime.UtcNow.Date;
        private string _TimeZone;
        public string TimeZone
        {
            get => _TimeZone;
            set
            {
                if (_TimeZone != value)
                {
                    _TimeZone = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeZone")); // reports this property
                }
            }
        }
        public bool ConferenceData { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

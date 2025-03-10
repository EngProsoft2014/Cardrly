

namespace Cardrly.Models.Calendar
{
    public class CalendarGmailRequest
    {
        public string? Summary { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? Attendees { get; set; }
        public DateTime Start { get; set; } = DateTime.UtcNow.AddDays(1);
        public DateTime End { get; set; } = DateTime.UtcNow.AddDays(1).AddHours(1);
        public string? TimeZone { get; set; }
        public bool ConferenceData { get; set; }
    }
}

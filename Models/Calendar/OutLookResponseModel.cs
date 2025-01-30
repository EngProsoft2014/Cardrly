using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Cardrly.Models.Calendar.GmailResponseModel;

namespace Cardrly.Models.Calendar
{
    public class OutLookResponseModel
    {
        public class CalendarOutLookResponse
        {
            public List<CalendarOutlookEvent> Events { get; set; }
        }

        public class CalendarOutlookEvent
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("subject")]
            public string Subject { get; set; }
            public DateTimeInfo Start { get; set; }

            [JsonProperty("end")]
            public DateTimeInfo End { get; set; }

            [JsonProperty("location")]
            public LocationOutlook Location { get; set; }

            [JsonProperty("onlineMeeting")]
            public OnlineMeeting OnlineMeeting { get; set; }

            [JsonProperty("attendees")]
            public List<Attendee> Attendees { get; set; }

            [JsonProperty("summary")]
            public string Summary { get; set; }

            [JsonProperty("bodyPreview")]
            public string bodyPreview { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("conferenceData")]
            public ConferenceData ConferenceData { get; set; }

            [JsonProperty("htmlLink")]
            public string HtmlLink { get; set; }
        }

        public class OnlineMeeting
        {
            [JsonProperty("joinUrl")]
            public string JoinUrl { get; set; }
        }

        public class DateTimeInfo
        {
            public DateTime DateTime { get; set; }

            [JsonProperty("timeZone")]
            public string TimeZone { get; set; }
        }

        public class LocationOutlook
        {
            [JsonProperty("displayName")]
            public string DisplayName { get; set; }
        }

        public class Attendee
        {
            [JsonProperty("emailAddress")]
            public EmailAddressOutLook EmailAddress { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public class EmailAddressOutLook
        {
            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

    }
}

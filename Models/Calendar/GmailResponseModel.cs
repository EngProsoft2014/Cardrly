using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Calendar
{
    public class GmailResponseModel
    {
        public class CalendarGmailResponse
        {
            [JsonProperty("kind")]
            public string Kind { get; set; }

            [JsonProperty("etag")]
            public string Etag { get; set; }

            [JsonProperty("summary")]
            public string Summary { get; set; }

            [JsonProperty("timeZone")]
            public string TimeZone { get; set; }
            public List<CalendarEventGmail> Items { get; set; }
        }

        public class CalendarEventGmail
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("summary")]
            public string Summary { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("location")]
            public string Location { get; set; }
            public EventDateTime Start { get; set; }

            [JsonProperty("end")]
            public EventDateTime End { get; set; }

            [JsonProperty("attendees")]
            public List<AttendeeGmail> Attendees { get; set; }

            [JsonProperty("conferenceData")]
            public ConferenceData ConferenceData { get; set; } = new ConferenceData();

            [JsonProperty("htmlLink")]
            public string HtmlLink { get; set; }
        }

        public class EventDateTime
        {
            public DateTime DateTime { get; set; }

            [JsonProperty("timeZone")]
            public string TimeZone { get; set; }
        }

        public class AttendeeGmail
        {
            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("responseStatus")]
            public string ResponseStatus { get; set; }
        }

        public class ConferenceData
        {
            [JsonProperty("entryPoints")]
            public List<EntryPoint> EntryPoints { get; set; } = new List<EntryPoint>();
        }

        public class EntryPoint
        {
            [JsonProperty("entryPointType")]
            public string EntryPointType { get; set; }

            [JsonProperty("uri")]
            public string Uri { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }
        }

    }
}

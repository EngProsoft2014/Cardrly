using Newtonsoft.Json;

namespace Cardrly.Models.Calendar
{
    public class CalendlyResponseModel
    {
        public class CalendarCalendlyResponse
        {
            public CalendarEvent CalendarEvent { get; set; }
            public string CreatedAt { get; set; }

            [JsonProperty("end_time")]
            public DateTime end_time { get; set; }

            [JsonProperty("event_memberships")]
            public List<EventMembership> EventMemberships { get; set; }
            public string EventType { get; set; }
            public InviteesCounter InviteesCounter { get; set; }
            public Location Location { get; set; }
            public string MeetingNotesHtml { get; set; }
            public string MeetingNotesPlain { get; set; }
            public string Name { get; set; }

            [JsonProperty("start_time")]
            public DateTime start_time { get; set; }
            public string Status { get; set; }
            public string UpdatedAt { get; set; }

            [JsonProperty("uri")]
            public string Uri { get; set; }
            public List<Invitee> Invitees { get; set; } // List of Invitees for the event 
        }

        // Models
        public class CalendarEvent
        {
            public string ExternalId { get; set; }
            public string Kind { get; set; }
        }

        public class EventMembership
        {
            [JsonProperty("buffered_end_time")]
            public string BufferedEndTime { get; set; }

            [JsonProperty("buffered_start_time")]
            public string BufferedStartTime { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }

            [JsonProperty("user_email")]
            public string UserEmail { get; set; }

            [JsonProperty("user_name")]
            public string UserName { get; set; }
        }

        public class InviteesCounter
        {
            public int Active { get; set; }
            public int Limit { get; set; }
            public int Total { get; set; }
        }

        public class Location
        {
            public string join_url { get; set; }
            public string Status { get; set; }
            public string Type { get; set; }
        }

        public class QuestionAndAnswer
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }

        public class Invitee
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public string Timezone { get; set; }
            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
            public List<QuestionAndAnswer> QuestionsAndAnswers { get; set; } // Add this property
        }

        public class CalendlyRoot
        {
            public List<CalendarCalendlyResponse> Collection { get; set; }
        }
    }
}

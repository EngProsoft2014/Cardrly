using System.Text.RegularExpressions;

namespace Cardrly.Models.MeetingAiActionRecord
{
    public class MeetingAiActionRecordResponse : AudioItem
    {
        public string Id { get; set; } = default!;
        public string? AudioName { get; set; }
        public string? AudioUrl { get; set; }
        public string? AudioTime { get; set; }
        public string? AudioScript { get; set; }
        public string GoogleDriveFileId { get; set; } = default!;
        public string MeetingAiActionId { get; set; } = default!;
        public string? MeetingAiActionRecordAnalyzeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsScript { get; set; } = false;
    }
}

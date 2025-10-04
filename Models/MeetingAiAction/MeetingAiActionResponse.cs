namespace Cardrly.Models.MeetingAiAction
{
    public class MeetingAiActionResponse
    {
        public string Id { get; set; } = default!;
        public string MeetingAiMainId { get; set; } = default!;
        public string title { get; set; } = default!;
        public string GoogleDriveFolderId { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
    }
}
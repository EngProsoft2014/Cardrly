namespace Cardrly.Models.MeetingAiActionRecordAnalyze
{
    public class MeetingAiActionRecordAnalyzeResponse
    {
        public string Id { get; set; } = default!;
        public string AnalyzeScript { get; set; } = "";
        public string AudioAllScript { get; set; } = "";
        public DateTime CreatedDate { get; set; }

    }
}

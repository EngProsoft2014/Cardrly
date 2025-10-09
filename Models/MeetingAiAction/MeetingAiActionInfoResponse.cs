using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.Models.MeetingAiActionRecordAnalyze;
using System.Collections.ObjectModel;

namespace Cardrly.Models.MeetingAiAction
{
    public class MeetingAiActionInfoResponse
    {
        public string Id { get; set; } = default!;
        public string MeetingAiMainId { get; set; } = default!;
        public string title { get; set; } = default!;
        public string GoogleDriveFolderId { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public string SecretKey { get; set; }
        public MeetingAiActionRecordAnalyzeResponse MeetingAiActionRecordAnalyzeResponse { get; set; }= default!;
        public ObservableCollection<MeetingAiActionRecordResponse> MeetingAiActionRecords { get; set; } = [];
    }
}

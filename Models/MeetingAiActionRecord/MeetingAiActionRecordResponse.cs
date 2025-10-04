using System.Text.RegularExpressions;

namespace Cardrly.Models.MeetingAiActionRecord
{
    public class MeetingAiActionRecordResponse : AudioItem
    {
        public string Id { get; set; } = default!;
        public string? AudioName { get; set; }
        public string? AudioUrl { get; set; }
        //public string? AudioUrlVM { get { return !string.IsNullOrEmpty(AudioUrl) ? ConvertDriveUrl(AudioUrl) : null; } }
        public string? AudioTime { get; set; }
        public string? AudioScript { get; set; }
        public string GoogleDriveFileId { get; set; } = default!;
        public string MeetingAiActionId { get; set; } = default!;
        public string? MeetingAiActionRecordAnalyzeId { get; set; }
        public DateTime CreatedDate { get; set; }
        //public bool? IsScript { get{ return !string.IsNullOrEmpty(AudioScript) ? true : false; } set{ } }
        public bool? IsScript { get; set; }


        //string ConvertDriveUrl(string url)
        //{
        //    var match = Regex.Match(url, @"[-\w]{25,}");
        //    return match.Success
        //        ? $"https://drive.google.com/uc?export=open&id={match.Value}"
        //        : url;
        //}

    }
}

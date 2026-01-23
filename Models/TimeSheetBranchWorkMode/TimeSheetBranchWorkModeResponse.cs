namespace Cardrly.Models.TimeSheetBranchWorkMode
{
    public class TimeSheetBranchWorkModeResponse
    {
        public string Id { get; set; } = default!;
        public string? TimeSheetBranchesId { get; set; } = default!;
        public string AccountId { get; set; } = default!;
        public string WorkType { get; set; } = default!;
        public TimeSpan WorkStart { get; set; } = default!;
        public TimeSpan WorkEnd { get; set; } = default!;
    }
}

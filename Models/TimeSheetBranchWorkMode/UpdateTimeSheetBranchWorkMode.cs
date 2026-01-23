namespace Cardrly.Models.TimeSheetBranchWorkMode
{
    public class UpdateTimeSheetBranchWorkMode
    {
        public string? WorkType { get; set; } = default!;
        public TimeSpan? WorkStart { get; set; } = default!;
        public TimeSpan? WorkEnd { get; set; } = default!;
    }
}

using Cardrly.Enums;

namespace Cardrly.Models.TimeSheetBreak
{
    public class UpdateTimeSheetBreak
    {
        public string? Reason { get; set; } = default!;
        public BreakStatus? Status { get; set; } = BreakStatus.Pending;
        public DateTime? WorkDate { get; set; } = default!;
        public TimeSpan? HoursFrom { get; set; } = default!;
        public TimeSpan? HoursTo { get; set; } = default!;
        public double? DurationHours { get; set; } = default!;
        public int? DurationMinutes { get; set; } = default!;
    }
}

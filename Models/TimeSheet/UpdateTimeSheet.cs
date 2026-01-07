using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.TimeSheet
{
    public class UpdateTimeSheet
    {
        public string? TimeSheetBranchId { get; set; } = default!;
        public string? Notes { get; set; } = default!;
        public DateTime? WorkDate { get; set; } = default!;
        public TimeSpan? HoursFrom { get; set; } = default!;
        public TimeSpan? HoursTo { get; set; } = default!;
        public double? DurationHours { get; set; } = default!;
        public int? DurationMinutes { get; set; } = default!;
        public string? CheckinAddress { get; set; } = default!;
        public string? CheckoutAddress { get; set; } = default!;
        public double? TotalBreakHours { get; set; } = default!;
        public int? TotalBreakMinutes { get; set; } = default!;
    }
}

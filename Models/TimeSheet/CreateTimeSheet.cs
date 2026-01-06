using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.TimeSheet
{
    public class CreateTimeSheet
    {
        public string CardId { get; set; } = default!;
        public string? TimeSheetBranchId { get; set; } = default!;
        public DateTime WorkDate { get; set; } = default!;
        public TimeSpan HoursFrom { get; set; } = default!;
        public string CheckinAddress { get; set; } = default!;

    }
}

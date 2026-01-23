using Cardrly.Enums;

namespace Cardrly.Models.TimeSheetBreak
{
    public class GetTimeSheetBreakFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int Pagesize { get; set; } = 10;
        public string? rangetype { get; set; } = "Created-date";
        public DateOnly? fromdt { get; set; }
        public DateOnly? todt { get; set; }
        public BreakStatus? status { get; set; } = BreakStatus.Pending;
        public string? TimeSheetId { get; set; } = "";

    }
}

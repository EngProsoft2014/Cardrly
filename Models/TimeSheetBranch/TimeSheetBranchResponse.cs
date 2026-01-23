
using Cardrly.Models.TimeSheet;
using Cardrly.Models.TimeSheetBranchNetwork;
using Cardrly.Models.TimeSheetBranchWorkMode;
using Cardrly.Models.TimeSheetEmployeeBranch;

namespace Cardrly.Models.TimeSheetBranch
{
    public class TimeSheetBranchResponse
    {
        public string Id { get; set; } = default!;
        public string AccountId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        public double Latitude { get; set; } = default!;
        public double Longitude { get; set; } = default!;
        public List<TimeSheetResponse>? TimeSheets { get; set; } = [];
        public List<TimeSheetEmployeeBranchResponse>? TimeSheetEmployeeBranches { get; set; } = [];
        public List<TimeSheetBranchWorkModeResponse>? WorkModes { get; set; } = [];
        public List<TimeSheetBranchNetworkResponse>? Networks { get; set; } = [];
    }
}

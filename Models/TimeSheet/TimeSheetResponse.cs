using Cardrly.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.TimeSheet
{
    public class TimeSheetResponse
    {
        public string Id { get; set; } = default!;
        // From Card
        public string CardId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public string AccountId { get; set; } = default!;

        // From TimeSheet
        public string? TimeSheetId { get; set; }
        public string? TimeSheetBranchId { get; set; }
        public string? TimeSheetBranchName { get; set; }
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
        public bool? IsBreak { get; set; } = default!;

        // From TimeSheetEmployeeBranch
        public string? TimeSheetEmployeeBranchId { get; set; }
        public bool NeedApprovedBreak { get; set; }
        public bool IsUseTimeSheet { get; set; }
        public bool IsUseTracking { get; set; }
        public bool IsUseNetwork { get; set; }
        public bool IsUseDeviceId { get; set; }
        public bool IsUseFaceId { get; set; }
        public string? DeviceId { get; set; }
        public string? FaceId { get; set; }

        // 🔥 Work mode info
        public string? TimeSheetBranchWorkModeId { get; set; }
        public string? WorkType { get; set; }
        public TimeSpan? WorkStart { get; set; }
        public TimeSpan? WorkEnd { get; set; }

        // 🔥 Network info
        public string? TimeSheetBranchNetworkId { get; set; }
        public string? NetworkName { get; set; }
        public string? NetworkIpAddress { get; set; }

        public bool IsShowBaseCheckIn {  get{ return ((HoursFrom == null && UserId == Preferences.Default.Get(ApiConstants.userid, "")) || HoursFrom != null && UserId != Preferences.Default.Get(ApiConstants.userid, "")) ? true : false; } set{ } } 
        public bool IsShowBaseCheckOut { get { return HoursFrom != null ? true : false; } set { } }
        public bool IsShowBaseBreakIn { get { return (HoursFrom != null && IsBreak == false) ? true : false; } set { } }
        public bool IsShowBaseBreakOut { get { return (HoursFrom != null && IsBreak == true) ? true : false; } set { } }
    }
}

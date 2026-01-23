namespace Cardrly.Models.TimeSheetEmployeeBranch
{
    public class CreateTimeSheetEmployeeBranch
    {
        public bool NeedApprovedBreak { get; set; } = false;
        public bool IsUseTimeSheet { get; set; } = false;
        public bool IsUseTracking { get; set; } = false;
        public string? DeviceId { get; set; } = default!;
        public string? FaceId { get; set; } = default!;
        public bool IsUseNetwork { get; set; } = false;
        public bool IsUseDeviceId { get; set; } = false;
        public bool IsUseFaceId { get; set; } = false;
    }
}

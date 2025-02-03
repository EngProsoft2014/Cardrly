using Cardrly.Enums;

namespace Cardrly.Models.Devices
{
    public class DevicesResponse
    {
        public string Id { get; set; } = default!;
        public string AccountId { get; set; } = default!;
        public string CardId { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public string DeviceId { get; set; } = string.Empty;
        public EnumDeviceType DeviceType { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; } = default!;
        public string? ImageUpload { get; set; } = default!;
        public string? UrlImageUpload { get; set; } = default!;
        public string? UrlImageUploadVM { get { return $"{Helpers.Utility.ServerUrl}{UrlImageUpload}"; } }
    }
}

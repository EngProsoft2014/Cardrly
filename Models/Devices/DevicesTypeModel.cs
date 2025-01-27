namespace Cardrly.Models.Devices
{
    public class DevicesTypeModel
    {
        public int DeviceNumber { get; set; }

        public string? DeviceName { get; set; }

        public string? DeviceImgUrl { get; set; }
        public string? DeviceImgUrlVM { get { return $"https://app.cardrly.com/{DeviceImgUrl}"; } }
    }
}

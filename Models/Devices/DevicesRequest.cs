﻿using Cardrly.Enums;

namespace Cardrly.Models.Devices
{
    public record DevicesRequest
    {
        public string DeviceId { get; set; } = string.Empty;

        public EnumDeviceType DeviceType { get; set; }

        public string RedirectUrl { get; set; } = string.Empty;
    }
}
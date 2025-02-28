
using Cardrly.Resources.Lan;

namespace Cardrly.Services
{
    public class SecurityService : ISecurityService
    {
        private CancellationTokenSource _cancellationTokenSource;

        public event Action<bool,string> SecurityStatusChanged;
        string message = string.Empty;

        public async Task StartSecurityMonitoring()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    bool isSecure = await IsDeviceSecure();
                    SecurityStatusChanged?.Invoke(isSecure , message);
                    message = string.Empty;
                    await Task.Delay(3000); // Check every 3 seconds
                }
            });
        }

        public void StopSecurityMonitoring()
        {
            _cancellationTokenSource?.Cancel();
        }
        public async Task<bool> IsDeviceSecure()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                if (App.Services.GetService<IRootChecker>().IsDeviceRooted())
                    message = $"{AppResources.msgDeviceRooted}";
                else if (App.Services.GetService<IDeveloperOptionsChecker>().IsDeveloperOptionsEnabled())
                    message = $"{AppResources.msgDeveloperOptionOn}";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                if (App.Services.GetService<IJailbreakChecker>().IsDeviceJailbroken())
                    message = $"{AppResources.msgDevicejailbroken}";

            }

            if (!string.IsNullOrEmpty(message))
            {
                return true;
            }

            return false;
        }
    }
}

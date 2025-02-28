
namespace Cardrly.Services
{
    public interface ISecurityService
    {
        event Action<bool,string> SecurityStatusChanged; // Event when security status changes
        Task<bool> IsDeviceSecure();
        public Task StartSecurityMonitoring(); // Start monitoring security status
        void StopSecurityMonitoring();
    }
}

using Cardrly.Services;
using Android.Provider;
using Cardrly.Platforms.Android;
[assembly: Dependency(typeof(DeveloperOptionsChecker))]
namespace Cardrly.Platforms.Android
{
    public class DeveloperOptionsChecker : IDeveloperOptionsChecker
    {
        public bool IsDeveloperOptionsEnabled()
        {
            var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.ApplicationContext;

            if (context == null)
                return false;

            return Settings.Global.GetInt(context.ContentResolver, Settings.Global.AdbEnabled, 0) == 1;
        }
    }
}

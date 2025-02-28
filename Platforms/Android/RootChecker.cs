using Android.OS;
using Cardrly.Platforms.Android;
using Cardrly.Services;
using Java.IO;

[assembly: Dependency(typeof(RootChecker))]
namespace Cardrly.Platforms.Android
{
    public class RootChecker : IRootChecker
    {
        public bool IsDeviceRooted()
        {
            return CheckRootMethod1() || CheckRootMethod2() || CheckRootMethod3();
        }

        private bool CheckRootMethod1()
        {
            // Check for common root binaries
            string[] paths =
            {
                "/system/app/Superuser.apk",
                "/sbin/su",
                "/system/bin/su",
                "/system/xbin/su",
                "/data/local/xbin/su",
                "/data/local/bin/su",
                "/system/sd/xbin/su",
                "/system/bin/failsafe/su",
                "/data/local/su"
            };

            foreach (string path in paths)
            {
                if (System.IO.File.Exists(path))
                    return true;
            }
            return false;
        }

        private bool CheckRootMethod2()
        {
            try
            {
                var process = Java.Lang.Runtime.GetRuntime().Exec(new string[] { "/system/xbin/which", "su" });
                var reader = new BufferedReader(new InputStreamReader(process.InputStream));
                return reader.ReadLine() != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CheckRootMethod3()
        {
            return Build.Tags.Contains("test-keys");
        }
    }
}


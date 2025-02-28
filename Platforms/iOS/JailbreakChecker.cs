using Cardrly.Platforms.iOS;
using Cardrly.Services;
using Foundation;

[assembly: Dependency(typeof(JailbreakChecker))]
namespace Cardrly.Platforms.iOS
{
    public class JailbreakChecker : IJailbreakChecker
    {
        public bool IsDeviceJailbroken()
        {
            return CheckSuspiciousPaths() || CanWriteOutsideSandbox();
        }

        private bool CheckSuspiciousPaths()
        {
            string[] paths =
            {
                "/Applications/Cydia.app",
                "/Library/MobileSubstrate/MobileSubstrate.dylib",
                "/bin/bash",
                "/usr/sbin/sshd",
                "/etc/apt"
            };

            foreach (string path in paths)
            {
                if (NSFileManager.DefaultManager.FileExists(path))
                    return true;
            }
            return false;
        }

        private bool CanWriteOutsideSandbox()
        {
            try
            {
                System.IO.File.WriteAllText("/private/jailbreak.txt", "test");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

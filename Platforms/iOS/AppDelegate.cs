using Foundation;
using UIKit;
using UserNotifications;

namespace Cardrly
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.WriteLine($"iOS Exception: {e.ExceptionObject}");
            };

            var result = base.FinishedLaunching(application, launchOptions);

            // Get the main window
            var window = UIApplication.SharedApplication.KeyWindow;

            if (window != null)
            {
                // Add a tap gesture recognizer to dismiss the keyboard
                var tapRecognizer = new UITapGestureRecognizer(() =>
                {
                    window.EndEditing(true); // Dismiss the keyboard
                })
                {
                    CancelsTouchesInView = false // Ensure other UI interactions are not blocked
                };
                window.AddGestureRecognizer(tapRecognizer);
            }

            // Request permission for notifications
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                (granted, error) =>
                {
                    if (granted)
                    {
                        Console.WriteLine("Notification permission granted.");
                        InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
                    }
                    else
                    {
                        Console.WriteLine("Notification permission denied.");
                    }
                });

            return result;
        }
    }
}

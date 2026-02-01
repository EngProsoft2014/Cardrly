using Cardrly.Platforms.iOS;
using Cardrly.Platforms.iOS.Services;
using Cardrly.Services.Data;
using Foundation;
using UIKit;
using UserNotifications;

namespace Cardrly
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        public static Action? BackgroundSessionCompletionHandler { get; set; }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.WriteLine($"iOS Exception: {e.ExceptionObject}");
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Console.WriteLine($"Unobserved Task Exception: {e.Exception}");
                e.SetObserved();
            };

            var result = base.FinishedLaunching(application, launchOptions);

            // Reset background location state at app startup
            var locationService = new iOSLocationTrackingService(new SignalRService());
            locationService.StopBackgroundTracking();

            // Get the main window
            // Get the key window safely (iOS 13+)
            var window = UIApplication.SharedApplication.Windows.FirstOrDefault(w => w.IsKeyWindow);

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

            // 📡 Listen for background session completion
            NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("NSURLSessionDidFinishEventsForBackgroundURLSessionNotification"),
                notification =>
                {
                    Console.WriteLine("📱 Background session finished events received.");

                    if (BackgroundSessionCompletionHandler != null)
                    {
                        BackgroundSessionCompletionHandler.Invoke();
                        BackgroundSessionCompletionHandler = null;
                    }

                    // Optional: show local notification when upload completes
                    var content = new UNMutableNotificationContent
                    {
                        Title = "Upload Complete",
                        Body = "Your meeting recording has been successfully uploaded.",
                        Sound = UNNotificationSound.Default
                    };

                    var request = UNNotificationRequest.FromIdentifier(
                        Guid.NewGuid().ToString(),
                        content,
                        null
                    );

                    UNUserNotificationCenter.Current.AddNotificationRequest(request, null);
                });

            return result;
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);

            // Optional: reset again when app comes to foreground
            var locationService = new iOSLocationTrackingService(new SignalRService());
            locationService.StopBackgroundTracking();
        }


        // ✅ This is the correct override in .NET 8 MAUI
        [Export("application:handleEventsForBackgroundURLSession:completionHandler:")]
        public void HandleEventsForBackgroundUrlSession(UIApplication application, string sessionIdentifier, Action completionHandler)
        {
            Console.WriteLine($"📡 Background upload session reconnected: {sessionIdentifier}");
            BackgroundSessionCompletionHandler = completionHandler;

            // Recreate session with same ID
            BackgroundUploader.Instance.RecreateSession();

            BackgroundUploader.Instance.SetCompletionHandler(completionHandler);
        }

    }
}

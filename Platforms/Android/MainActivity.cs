using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Cardrly.Platforms.Android;
using Cardrly.Services;
using Plugin.NFC;
//using Plugin.Firebase.CloudMessaging;
namespace Cardrly
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            // Plugin NFC : Initialisation
            CrossNFC.Init(this);

            base.OnCreate(savedInstanceState);

            Microsoft.Maui.ApplicationModel.Platform.Init(this, savedInstanceState);

            this.Window?.AddFlags(WindowManagerFlags.Fullscreen);

            HandleIntent(Intent);
            CreateNotificationChannelIfNeeded();
            //DependencyInjection.ControlsBackground();

            // Handle uncaught exceptions
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, e) =>
            {
                Console.WriteLine($"Android Exception: {e.Exception.Message}");
                e.Handled = true;
            };

            //CreateNotificationFromIntent(Intent);

            //Request Notification Permission
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.PostNotifications }, 0);
                }
            }

            await RequestContactsPermission();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Plugin NFC: Restart NFC listening on resume (needed for Android 10+) 
            CrossNFC.OnResume();

            // 🔥 App reopened → stop reminder loop + notifications
            Cardrly.Platforms.Android.Services.LocationService.ClearReminders(this);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // Plugin NFC: Tag Discovery Interception
            CrossNFC.OnNewIntent(intent);
            HandleIntent(intent);
            //CreateNotificationFromIntent(intent);
        }

        protected override void AttachBaseContext(Context? @base)
        {
            Configuration configuration = new(@base!.Resources!.Configuration)
            {
                FontScale = 1.0f
            };
            ApplyOverrideConfiguration(configuration);
            base.AttachBaseContext(@base);
        }

        public async Task<bool> RequestContactsPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteContacts) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteContacts }, 1);
                return false;
            }
            return true;
        }

        private static void HandleIntent(Intent intent)
        {
            //FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }

        private void CreateNotificationChannelIfNeeded()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                CreateNotificationChannel();
            }
        }

        private void CreateNotificationChannel()
        {
            var channelId = $"{PackageName}.general";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);
            //FirebaseCloudMessagingImplementation.ChannelId = channelId;
        }

        static void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(Cardrly.Platforms.Android.NotificationManagerService.TitleKey);
                string message = intent.GetStringExtra(Cardrly.Platforms.Android.NotificationManagerService.MessageKey);

                var service = IPlatformApplication.Current.Services.GetService<INotificationManagerService>();
                service.ReceiveNotification(title, message);
            }
        }

        public void StartUploadInForegroundService(string filePath, string apiUrl, string token)
        {
            var intent = new Intent(this, typeof(UploadForegroundService));
            intent.PutExtra("filePath", filePath);
            intent.PutExtra("apiUrl", apiUrl);
            intent.PutExtra("token", token);
            StartForegroundService(intent);
        }
    }
}

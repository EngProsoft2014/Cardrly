
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using Cardrly.Helpers;
using Cardrly.Models.MeetingAiActionRecord;
using System.Net.Http.Headers;

namespace Cardrly.Platforms.Android
{
    [Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class UploadForegroundService : Service
    {
        const string ChannelId = "upload_channel";
        const int NotificationId = 1001;

        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannel();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Task.Run(async () =>
            {
                try
                {
                    string filePath = intent.GetStringExtra("filePath");
                    string apiUrl = intent.GetStringExtra("apiUrl");
                    string token = intent.GetStringExtra("token");

                    StartForeground(NotificationId, BuildNotification("Uploading..."));

                    // 🔥 Use your existing upload method here directly
                    await new GenericRepository().PostFileWithFormAsync<object>(apiUrl, new AudioUploadRequest
                    {
                        AudioPath = filePath,
                        Extension = Path.GetExtension(filePath)
                    }, token);

                    StopForeground(true);
                    StopSelf();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Upload failed in service: {ex.Message}");
                    StopForeground(true);
                    StopSelf();
                }
            });

            return StartCommandResult.Sticky;
        }

        private Notification BuildNotification(string message)
        {
            return new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle("FixProUs")
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.upload)
                .SetOngoing(true)
                .Build();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, "Uploads", NotificationImportance.Low);
                var manager = (NotificationManager)GetSystemService(NotificationService);
                manager.CreateNotificationChannel(channel);
            }
        }

        public override IBinder OnBind(Intent intent) => null!;
    }
}

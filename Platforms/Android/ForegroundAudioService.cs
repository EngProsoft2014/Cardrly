using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Platforms.Android
{
    [Service(ForegroundServiceType = ForegroundService.TypeMicrophone, Exported = false)]
    public class ForegroundAudioService : Service
    {
        public const int SERVICE_ID = 1010;

        public override IBinder? OnBind(Intent? intent) => null;

        public override void OnCreate()
        {
            base.OnCreate();

            var channelId = "recording_channel";
            var channelName = "Audio Recording";

            var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Low);
                notificationManager.CreateNotificationChannel(channel);
            }

            var notification = new NotificationCompat.Builder(this, channelId)
                .SetContentTitle("Recording in progress")
                .SetContentText("Your meeting audio is being recorded.")
                .SetSmallIcon(Resource.Drawable.mic) // create a small icon in Resources/drawable
                .SetOngoing(true)
                .Build();

            StartForeground(SERVICE_ID, notification);
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            StopForeground(true);
        }
    }
}

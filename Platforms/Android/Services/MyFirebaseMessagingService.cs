using Android.App;
using AndroidX.Core.App;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Platforms.Android.Services
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            var body = message.GetNotification()?.Body;
            // Show notification
            var builder = new NotificationCompat.Builder(this, "default")
                .SetContentTitle("New Message")
                .SetContentText(body)
                .SetSmallIcon(Resource.Drawable.notification);

            var manager = NotificationManagerCompat.From(this);
            manager.Notify(0, builder.Build());
        }

        public override void OnNewToken(string token)
        {
            // Send token to ASP.NET Core API
        }
    }

}

using Android.App;
using Android.Content;
using Android.Telephony;
using Cardrly.Services.NativeAudioRecorder; // adjust namespace
using System;

namespace Cardrly.Platforms.Android.Receivers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "android.intent.action.PHONE_STATE" })]
    public class PhoneCallReceiver : BroadcastReceiver
    {
        public static Action<bool>? OnCallStateChanged; // true = in call, false = idle

        public override void OnReceive(Context context, Intent intent)
        {
            string? state = intent.GetStringExtra(TelephonyManager.ExtraState);

            if (state == TelephonyManager.ExtraStateRinging ||
                state == TelephonyManager.ExtraStateOffhook)
            {
                // Incoming or active call
                OnCallStateChanged?.Invoke(true);
            }
            else if (state == TelephonyManager.ExtraStateIdle)
            {
                // Call ended or rejected
                OnCallStateChanged?.Invoke(false);
            }
        }
    }
}


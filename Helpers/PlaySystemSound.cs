#if ANDROID
using Android.Media;
using Android.Content;
using Android.App;
#elif IOS || MACCATALYST
using AudioToolbox;
using Foundation;
#endif

namespace Cardrly.Helpers
{
    public class PlaySystemSound
    {

        public static void PlaySound()
        {
#if ANDROID
            var context = Android.App.Application.Context;
            var alarm = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            var ringtone = RingtoneManager.GetRingtone(context, alarm);
            ringtone.Play();

#elif IOS || MACCATALYST
            //SystemSound.Vibrate.PlaySystemSound(); // or any predefined SystemSound

            // Load bundled sound file
            var soundUrl = NSUrl.FromFilename("ringtone.mp3"); // your sound file
            SystemSound sound = new SystemSound(soundUrl);
            sound.PlaySystemSound();
#endif
        }
    }
}

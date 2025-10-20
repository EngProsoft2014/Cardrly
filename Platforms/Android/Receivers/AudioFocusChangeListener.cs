using Android.Media;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Platforms.Android.Receivers
{
    public class AudioFocusChangeListener : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
    {
        public static Action<string>? OnAudioFocusChanged;

        public void OnAudioFocusChange([GeneratedEnum] AudioFocus focusChange)
        {
            switch (focusChange)
            {
                case AudioFocus.Loss:
                case AudioFocus.LossTransient:
                    OnAudioFocusChanged?.Invoke("LOST");
                    break;

                case AudioFocus.Gain:
                    OnAudioFocusChanged?.Invoke("GAIN");
                    break;
            }
        }
    }
}

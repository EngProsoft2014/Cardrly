#if IOS
using System;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using AudioToolbox;
using Microsoft.Maui.Storage;

namespace Cardrly.Services.NativeAudioRecorder
{
    public class iOSAudioRecorder : INativeAudioRecorder
    {
        private AVAudioRecorder recorder;
        private string currentFilePath;
        private NSObject interruptionObserver;

        public event Action OnInterruptionBegan;
        public event Action OnInterruptionEnded;
        public event Action<string> OnRecordingResumed;

        public bool IsRecording => recorder?.Recording ?? false;

        public async Task<bool> Start(string filePath)
        {
            try
            {
                currentFilePath = filePath;

                var permissionGranted = await RequestPermission();
                if (!permissionGranted)
                    return false;

                var audioSession = AVAudioSession.SharedInstance();
                NSError error = null;

                audioSession.SetCategory(
                    AVAudioSessionCategory.PlayAndRecord,
                    AVAudioSessionCategoryOptions.DefaultToSpeaker |
                    AVAudioSessionCategoryOptions.AllowBluetoothA2DP |
                    AVAudioSessionCategoryOptions.AllowBluetooth |
                    AVAudioSessionCategoryOptions.MixWithOthers |
                    AVAudioSessionCategoryOptions.AllowAirPlay |
                    AVAudioSessionCategoryOptions.InterruptSpokenAudioAndMixWithOthers,
                    out error);

                audioSession.SetMode(AVAudioSession.ModeDefault, out error);
                audioSession.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out error);

                // 🎧 Subscribe to interruptions (calls, WhatsApp, etc.)
                RegisterForInterruptionNotifications();

                var settings = new AudioSettings
                {
                    SampleRate = 16000,
                    NumberChannels = 1,
                    AudioQuality = AVAudioQuality.Medium,
                    Format = AudioFormatType.MPEG4AAC,
                    EncoderBitRate = 48000
                };

                recorder = AVAudioRecorder.Create(NSUrl.FromFilename(filePath), settings, out error);
                recorder.PrepareToRecord();

                return recorder.Record();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void RegisterForInterruptionNotifications()
        {
            if (interruptionObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(interruptionObserver);
                interruptionObserver = null;
            }

            interruptionObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                AVAudioSession.InterruptionNotification,
                async notification =>
                {
                    var userInfo = notification.UserInfo;
                    var typeValue = userInfo?["AVAudioSessionInterruptionTypeKey"] as NSNumber;
                    if (typeValue == null)
                        return;

                    var type = (AVAudioSessionInterruptionType)typeValue.Int32Value;

                    if (type == AVAudioSessionInterruptionType.Began)
                    {
                        if (recorder?.Recording == true)
                            recorder.Stop();

                        OnInterruptionBegan?.Invoke();
                    }
                    else if (type == AVAudioSessionInterruptionType.Ended)
                    {
                        NSError error;
                        var session = AVAudioSession.SharedInstance();
                        session.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out error);

                        if (error != null)
                        {
                            return;
                        }

                        // Create new file for resumed recording
                        var newPath = Path.Combine(FileSystem.AppDataDirectory, $"resume_{DateTime.Now:yyyyMMddHHmmss}.m4a");

                        var settings = new AudioSettings
                        {
                            SampleRate = 16000,
                            NumberChannels = 1,
                            AudioQuality = AVAudioQuality.Medium,
                            Format = AudioFormatType.MPEG4AAC,
                            EncoderBitRate = 48000
                        };

                        var newRecorder = AVAudioRecorder.Create(NSUrl.FromFilename(newPath), settings, out error);
                        newRecorder.PrepareToRecord();
                        newRecorder.Record();

                        recorder = newRecorder; // replace old instance
                        currentFilePath = newPath;

                        OnInterruptionEnded?.Invoke();
                        OnRecordingResumed?.Invoke(newPath);
                    }
                });
        }


        private static async Task<bool> RequestPermission()
        {
            var tcs = new TaskCompletionSource<bool>();
            AVAudioSession.SharedInstance().RequestRecordPermission(granted => tcs.SetResult(granted));
            return await tcs.Task;
        }

        public void Pause()
        {
            if (recorder?.Recording == true)
                recorder.Pause();
        }

        public bool Resume()
        {
            return recorder?.Record() ?? false;
        }

        public async Task<string> Stop()
        {
            try
            {
                if (recorder?.Recording == true)
                    recorder.Stop();

                recorder?.Dispose();
                recorder = null;

                if (interruptionObserver != null)
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(interruptionObserver);
                    interruptionObserver.Dispose();
                    interruptionObserver = null;
                }

                var audioSession = AVAudioSession.SharedInstance();
                audioSession.SetActive(false, out _);
                audioSession.SetCategory(AVAudioSessionCategory.Ambient); // releases focus cleanly
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in Stop(): {ex.Message}");
            }

            return currentFilePath ?? string.Empty;
        }

        public void Dispose()
        {
            try
            {
                recorder?.Dispose();
                recorder = null;

                if (interruptionObserver != null)
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(interruptionObserver);
                    interruptionObserver.Dispose();
                    interruptionObserver = null;
                }
            }
            catch { }
        }
    }
}
#endif

#if IOS
using System;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using AudioToolbox;
using Cardrly.Services.NativeAudioRecorder;

namespace Cardrly.Services.AudioRecord
{
    public class iOSAudioRecorder : INativeAudioRecorder
    {
        AVAudioRecorder? recorder;
        string? filePath;
        NSObject? interruptionObserver;

        public bool IsRecording => recorder?.Recording ?? false;

        public async Task<bool> Start(string path)
        {
            try
            {
                filePath = path;

                // 🎤 Request permission first
                var permissionGranted = await RequestPermission();
                if (!permissionGranted)
                    return false;

                // 🎧 Configure session
                //var session = AVAudioSession.SharedInstance();
                //session.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker, out _);
                //session.SetMode(AVAudioSession.ModeDefault, out _);
                //session.SetActive(true, out _);
                var session = AVAudioSession.SharedInstance();
                session.SetCategory(
                    AVAudioSessionCategory.PlayAndRecord,
                    AVAudioSessionCategoryOptions.MixWithOthers |
                    AVAudioSessionCategoryOptions.AllowBluetooth |
                    AVAudioSessionCategoryOptions.AllowBluetoothA2DP |
                    AVAudioSessionCategoryOptions.DefaultToSpeaker,
                    out _
                );
                session.SetMode(AVAudioSession.ModeMeasurement, out _);
                session.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out _);

                // 🎧 Subscribe to interruptions (e.g., phone calls)
                RegisterForInterruptionNotifications();

                // 🗂️ Create recorder
                var url = NSUrl.FromFilename(filePath);

                var settings = new AudioSettings
                {
                    SampleRate = 16000f,
                    NumberChannels = 1,
                    AudioQuality = AVAudioQuality.High,
                    Format = AudioFormatType.LinearPCM,
                    LinearPcmBitDepth = 16,
                    LinearPcmBigEndian = false,
                    LinearPcmFloat = false
                };

                NSError? err;
                recorder = AVAudioRecorder.Create(url, settings, out err);

                if (err != null)
                {
                    Console.WriteLine($"❌ Recorder init error: {err.LocalizedDescription}");
                    return false;
                }

                recorder.MeteringEnabled = true;
                recorder.PrepareToRecord();

                bool started = recorder.Record();
                if (!started)
                    Console.WriteLine("❌ Recorder failed to start recording");

                return started;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Start() Exception: {ex.Message}");
                return false;
            }
        }

        private void RegisterForInterruptionNotifications()
        {
            // Remove existing observer if re-registering
            if (interruptionObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(interruptionObserver);
                interruptionObserver = null;
            }

            interruptionObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                AVAudioSession.InterruptionNotification,
                notification =>
                {
                    var interruptionTypeNumber = notification.UserInfo?[AVAudioSession.InterruptionTypeKey] as NSNumber;
                    if (interruptionTypeNumber == null)
                        return;

                    var type = (AVAudioSessionInterruptionType)interruptionTypeNumber.Int32Value;

                    if (type == AVAudioSessionInterruptionType.Began)
                    {
                        Console.WriteLine("📞 Audio interrupted — pausing");
                        Pause();
                    }
                    else if (type == AVAudioSessionInterruptionType.Ended)
                    {
                        Console.WriteLine("📞 Interruption ended — resuming");
                        try
                        {
                            AVAudioSession.SharedInstance().SetActive(true, out _);
                            Resume();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Resume after interruption failed: {ex.Message}");
                        }
                    }
                });
        }

        public void Pause()
        {
            try
            {
                if (recorder?.Recording ?? false)
                    recorder?.Pause();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Pause() Exception: {ex.Message}");
            }
        }

        public bool Resume()
        {
            try
            {
                return recorder?.Record() ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Resume() Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<string> Stop()
        {
            try
            {
                recorder?.Stop();
                recorder?.Dispose();
                recorder = null;

                if (interruptionObserver != null)
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(interruptionObserver);
                    interruptionObserver = null;
                }

                var session = AVAudioSession.SharedInstance();
                session.SetActive(false, out _);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stop error: {ex.Message}");
            }

            return filePath ?? string.Empty;
        }

        private static async Task<bool> RequestPermission()
        {
            var tcs = new TaskCompletionSource<bool>();
            AVAudioSession.SharedInstance().RequestRecordPermission(granted => tcs.SetResult(granted));
            return await tcs.Task;
        }
    }
}
#endif

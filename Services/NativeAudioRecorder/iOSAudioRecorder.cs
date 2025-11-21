#if IOS
using AVFoundation;
using Concentus.Enums;
using Concentus.Structs;
using Foundation;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cardrly.Services.NativeAudioRecorder
{
    public class iOSAudioRecorder : INativeAudioRecorder, IDisposable
    {
        // Audio
        private AVAudioEngine engine;
        private AVAudioInputNode inputNode;
        private AVAudioFormat tapFormat;

        // Session / interruptions
        private NSObject interruptionObserver;

        // Opus
        private OpusEncoder encoder;
        private FileStream opusStream;
        private string currentFilePath;

        // Frame accumulation (20ms @ 48kHz mono = 960 samples)
        private const int OpusSampleRate = 48000;
        private const int OpusChannels = 1;
        private const int FrameSamples = 960;
        private short[] frameBuffer = new short[FrameSamples];
        private int frameFill = 0;

        public event Action OnInterruptionBegan;
        public event Action OnInterruptionEnded;
        public event Action<string> OnRecordingResumed;

        public bool IsRecording { get; private set; }

        public async Task<bool> Start(string filePath)
        {
            try
            {
                currentFilePath = Path.ChangeExtension(filePath, ".opus");

                var permissionGranted = await RequestPermission();
                if (!permissionGranted) return false;

                // Configure session
                var session = AVAudioSession.SharedInstance();
                session.SetCategory(AVAudioSessionCategory.PlayAndRecord,
                    AVAudioSessionCategoryOptions.DefaultToSpeaker |
                    AVAudioSessionCategoryOptions.AllowBluetooth |
                    AVAudioSessionCategoryOptions.AllowBluetoothA2DP, out _);
                session.SetMode(AVAudioSession.ModeDefault, out _);
                session.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out _);

                RegisterForInterruptionNotifications();

                // Init Opus encoder and output stream
                encoder = new OpusEncoder(OpusSampleRate, OpusChannels, OpusApplication.OPUS_APPLICATION_AUDIO)
                {
                    Bitrate = 16000 // 16 kbps: great for speech, tiny files
                };
                opusStream = File.Create(currentFilePath);

                // Setup engine & tap
                engine = new AVAudioEngine();
                inputNode = engine.InputNode;

                // Ask input node to deliver mono float 32 at 48kHz (engine often uses float format)
                tapFormat = new AVAudioFormat(OpusSampleRate, OpusChannels); // non-interleaved float by default

                // Install tap: bufferSize in frames; e.g., 1024 is fine, we’ll re‑chunk to 960
                inputNode.InstallTapOnBus(0, 1024, tapFormat, (buffer, when) =>
                {
                    int frameCount = (int)buffer.FrameLength;
                    int channels = (int)buffer.Format.ChannelCount;

                    // FloatChannelData is IntPtr (pointer to float samples)
                    var floatPtr = buffer.FloatChannelData;
                    if (floatPtr == IntPtr.Zero) return;

                    // Copy unmanaged floats into managed array
                    float[] samples = new float[frameCount];
                    Marshal.Copy(floatPtr, samples, 0, frameCount);

                    for (int i = 0; i < frameCount; i++)
                    {
                        // Convert float [-1,1] → 16-bit PCM
                        int s = (int)(samples[i] * short.MaxValue);
                        if (s > short.MaxValue) s = short.MaxValue;
                        if (s < short.MinValue) s = short.MinValue;
                        short pcm = (short)s;

                        frameBuffer[frameFill++] = pcm;

                        if (frameFill == FrameSamples)
                        {
                            byte[] opusBuf = new byte[4000];
                            int encoded = encoder.Encode(frameBuffer, 0, FrameSamples, opusBuf, 0, opusBuf.Length);
                            opusStream.Write(opusBuf, 0, encoded);
                            frameFill = 0;
                        }
                    }
                });

                engine.Prepare();
                NSError startErr = null;
                engine.StartAndReturnError(out startErr);
                if (startErr != null) throw new Exception(startErr.LocalizedDescription);

                IsRecording = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LiveOpusRecorder.Start error: {ex.Message}");
                Dispose();
                return false;
            }
        }

        private void EncodeAndWriteFrame(short[] samples, int count)
        {
            // Encode a 20ms frame
            byte[] opusBuf = new byte[4000];
            int encoded = encoder.Encode(samples, 0, count, opusBuf, 0, opusBuf.Length);
            if (encoded > 0)
            {
                opusStream.Write(opusBuf, 0, encoded);
            }
        }

        public void Pause()
        {
            if (!IsRecording) return;
            engine?.Pause();
            IsRecording = false;
        }

        public bool Resume()
        {
            if (engine == null || IsRecording) return false;

            NSError err = null;
            engine.StartAndReturnError(out err);
            if (err != null) return false;

            IsRecording = true;
            return true;
        }

        public async Task<string> Stop()
        {
            try
            {
                if (IsRecording)
                {
                    // Flush partial frame if any: pad with zeros (optional)
                    if (frameFill > 0)
                    {
                        for (int i = frameFill; i < FrameSamples; i++) frameBuffer[i] = 0;
                        EncodeAndWriteFrame(frameBuffer, FrameSamples);
                        frameFill = 0;
                    }
                }

                inputNode?.RemoveTapOnBus(0);
                engine?.Stop();

                opusStream?.Flush();
                opusStream?.Dispose();
                opusStream = null;

                encoder = null;
                IsRecording = false;

                var session = AVAudioSession.SharedInstance();
                session.SetActive(false, out _);
                session.SetCategory(AVAudioSessionCategory.Ambient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stop error: {ex.Message}");
            }

            return currentFilePath ?? string.Empty;
        }

        private void RegisterForInterruptionNotifications()
        {
            interruptionObserver?.Dispose();
            interruptionObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                AVAudioSession.InterruptionNotification,
                notification =>
                {
                    var typeValue = notification.UserInfo?["AVAudioSessionInterruptionTypeKey"] as NSNumber;
                    if (typeValue == null) return;

                    var type = (AVAudioSessionInterruptionType)typeValue.Int32Value;

                    if (type == AVAudioSessionInterruptionType.Began)
                    {
                        // Stop engine and close current opus stream cleanly
                        try
                        {
                            Pause();
                            inputNode?.RemoveTapOnBus(0);
                            opusStream?.Flush();
                            opusStream?.Dispose();
                            opusStream = null;
                        }
                        catch { }

                        OnInterruptionBegan?.Invoke();
                    }
                    else if (type == AVAudioSessionInterruptionType.Ended)
                    {
                        // Reactivate session and resume with a new opus file
                        var session = AVAudioSession.SharedInstance();
                        session.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out var err);
                        if (err != null) return;

                        var newPath = Path.Combine(FileSystem.AppDataDirectory, $"resume_{DateTime.Now:yyyyMMddHHmmss}.opus");

                        // Recreate encoder & stream
                        encoder = new OpusEncoder(OpusSampleRate, OpusChannels, OpusApplication.OPUS_APPLICATION_AUDIO)
                        {
                            Bitrate = 16000
                        };
                        opusStream = File.Create(newPath);

                        // Reinstall tap
                        tapFormat = new AVAudioFormat(OpusSampleRate, OpusChannels);
                        inputNode.InstallTapOnBus(0, 1024, tapFormat, (buffer, when) =>
                        {
                            int frameCount = (int)buffer.FrameLength;
                            int channels = (int)buffer.Format.ChannelCount;

                            // FloatChannelData is IntPtr (pointer to float samples)
                            var floatPtr = buffer.FloatChannelData;
                            if (floatPtr == IntPtr.Zero) return;

                            // Copy unmanaged floats into managed array
                            float[] samples = new float[frameCount];
                            Marshal.Copy(floatPtr, samples, 0, frameCount);

                            for (int i = 0; i < frameCount; i++)
                            {
                                // Convert float [-1,1] → 16-bit PCM
                                int s = (int)(samples[i] * short.MaxValue);
                                if (s > short.MaxValue) s = short.MaxValue;
                                if (s < short.MinValue) s = short.MinValue;
                                short pcm = (short)s;

                                frameBuffer[frameFill++] = pcm;

                                if (frameFill == FrameSamples)
                                {
                                    byte[] opusBuf = new byte[4000];
                                    int encoded = encoder.Encode(frameBuffer, 0, FrameSamples, opusBuf, 0, opusBuf.Length);
                                    opusStream.Write(opusBuf, 0, encoded);
                                    frameFill = 0;
                                }
                            }
                        });



                        engine?.Prepare();
                        engine?.StartAndReturnError(out _);

                        currentFilePath = newPath;
                        IsRecording = true;

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

        public void Dispose()
        {
            try
            {
                inputNode?.RemoveTapOnBus(0);
                engine?.Stop();
                opusStream?.Dispose();
                interruptionObserver?.Dispose();
            }
            catch { }
        }
    }
}
#endif


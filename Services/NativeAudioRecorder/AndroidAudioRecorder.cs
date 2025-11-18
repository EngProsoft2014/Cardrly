#if ANDROID
using Android.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Services.NativeAudioRecorder
{
    public static class AndroidAudioRecorder
    {

        private static MediaRecorder _recorder;
        public static bool IsRecording => _recorder != null;

        public static Task<string> StartAsync(string filePath = null)
        {
            var path = filePath ?? Path.Combine(FileSystem.AppDataDirectory, $"rec_{DateTime.Now:yyyyMMddHHmmss}.m4a");

            _recorder = new MediaRecorder();
            _recorder.SetAudioSource(AudioSource.Mic);
            _recorder.SetOutputFormat(OutputFormat.Mpeg4);     // .m4a container
            _recorder.SetAudioEncoder(AudioEncoder.Aac);       // AAC encoder
            _recorder.SetAudioSamplingRate(16000);
            _recorder.SetAudioEncodingBitRate(64000);          // 64 kbps — tune for size/quality
            _recorder.SetAudioChannels(1);
            _recorder.SetOutputFile(path);
            _recorder.Prepare();
            _recorder.Start();

            return Task.FromResult(path);
        }

        public static Task StopAsync()
        {
            if (_recorder == null) return Task.CompletedTask;
            try { _recorder.Stop(); } catch { /* ignore stop errors */ }
            _recorder.Release();
            _recorder.Dispose();
            _recorder = null;
            return Task.CompletedTask;
        }
    }
}
#endif
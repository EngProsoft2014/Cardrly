#if ANDROID
using Android.Media;
using Cardrly.Models.MeetingAiActionRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Content;

namespace Cardrly.Services.AudioStream
{
    public class AndroidAudioService : IAudioStreamService
    {
        MediaPlayer? player;

        public void Play(MeetingAiActionRecordResponse audio)
        {
            player?.Release();
            player = new MediaPlayer();
            audio.AudioUrl = ConvertDriveUrl(audio.AudioUrl!);
            player.SetDataSource(audio.AudioUrl);  // direct URL (e.g. Google Drive direct link)
            player.PrepareAsync();
            player.Prepared += (s, e) => player.Start();
            audio.IsPlaying = true;
            //audio.StartWaveAnimation();
        }

        public void Stop()
        {
            player?.Stop();
            player?.Release();
            player = null;
        }

        public Task<MeetingAiActionRecordResponse> ReturnPlayAudio(MeetingAiActionRecordResponse audio)
        {
            Play(audio);
            return Task.FromResult(audio);
        }

        public Task<MeetingAiActionRecordResponse> ReturnStopAudio(MeetingAiActionRecordResponse audio)
        {
            Stop();
            audio.IsPlaying = false;
            //audio.StopWaveAnimation();
            audio.Player = null;
            return Task.FromResult(audio);
        }


        string ConvertDriveUrl(string url)
        {
            // Example input: https://drive.google.com/file/d/FILE_ID/view?usp=sharing
            var match = Regex.Match(url, @"[-\w]{25,}"); // extract FILE_ID
            if (!match.Success) return url;

            var fileId = match.Value;
            return $"https://drive.google.com/uc?export=download&id={fileId}";
        }

        public void Pause()
        {
            
        }
    }
}
#endif

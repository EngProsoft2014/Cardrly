#if IOS
using AVFoundation;
using Cardrly.Models.MeetingAiActionRecord;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cardrly.Services.AudioStream
{
    public class iOSAudioService : IAudioStreamService
    {
        AVPlayer? player;

        public void Play(MeetingAiActionRecordResponse audio)
        {
            audio.AudioUrl = ConvertDriveUrl(audio.AudioUrl!);
            player = AVPlayer.FromUrl(NSUrl.FromString(audio.AudioUrl)!);
            player.Play();
            audio.IsPlaying = true;
            //audio.StartWaveAnimation();
        }

        public void Stop()
        {
            player?.Pause();
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

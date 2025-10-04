using Cardrly.Models.MeetingAiActionRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Services.AudioStream
{
    public interface IAudioStreamService
    {
        void Play(MeetingAiActionRecordResponse audio);
        Task<MeetingAiActionRecordResponse> ReturnPlayAudio(MeetingAiActionRecordResponse audio);
        void Pause();
        void Stop();
        Task<MeetingAiActionRecordResponse> ReturnStopAudio(MeetingAiActionRecordResponse audio);
    }
}

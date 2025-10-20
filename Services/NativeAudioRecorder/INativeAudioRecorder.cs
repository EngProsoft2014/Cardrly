using Cardrly.Models.MeetingAiActionRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Services.NativeAudioRecorder
{
    public interface INativeAudioRecorder
    {
        bool IsRecording { get; }
        Task<bool> Start(string path);
        void Pause();
        bool Resume();
        Task<string> Stop();

        event Action OnInterruptionBegan;
        event Action OnInterruptionEnded;
        event Action<string> OnRecordingResumed;
    }
}

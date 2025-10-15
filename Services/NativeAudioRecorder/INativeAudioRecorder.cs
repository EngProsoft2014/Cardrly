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
        Task<bool> Start(string path); // start new recording
        void Pause();                  // pause but keep same file
        bool Resume();                 // resume recording
        Task<string> Stop();           // stop and return the file path
    }
}

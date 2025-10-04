using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.MeetingAiActionRecord
{
    public class AudioUploadRequest
    {
        public string AudioTime { get; set; }
        public byte[] AudioData { get; set; } // Audio data to be uploaded
        public string Extension { get; set; } // File extension of the image (e.g., ".jpg", ".png")
    }
}

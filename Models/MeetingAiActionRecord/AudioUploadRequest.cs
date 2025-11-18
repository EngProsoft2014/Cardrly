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
        public string AudioUploadId { get; set; }//for know it for return back upload
        public string AudioTime { get; set; }
        public string AudioPath { get; set; } 
        public string? AudioScript { get; set; }
        public List<MeetingMessage>? LstMeetingMessage { get; set; } // List of Speakers text in the meeting
        public string Extension { get; set; } // File extension of the image (e.g., ".jpg", ".png")

        public byte[]? AudioBytes { get; set; }
        //public List<string>? AudioParts { get; set; }
    }
}

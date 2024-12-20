using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cardrly.Models.Lead
{
    public class LeadRequest : INotifyPropertyChanged
    {
        public string FullName { get; set; } = default!;
        public string? Email { get; set; } = default!;
        public string? Address { get; set; } = default!;
        public string? Phone { get; set; } = default!;
        public string? Company { get; set; } = default!;
        public string? Website { get; set; } = default!;
        [JsonIgnore]
        ImageSource? _ImagefileProfile;
        [JsonIgnore]
        public ImageSource? ImagefileProfile
        {
            get
            {
                return _ImagefileProfile;
            }
            set
            {
                _ImagefileProfile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ImagefileProfile"));
                }
            }
        }
        public byte[]? ImgFile { get; set; }
        public string? Extension { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

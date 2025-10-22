using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.MeetingAiActionRecord
{
    public class MeetingMessage : INotifyPropertyChanged
    {
        private string _speaker;
        public string Speaker
        {
            get => _speaker;
            set
            {
                if (_speaker != value)
                {
                    _speaker = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _speakerDuration;
        public string SpeakerDuration
        {
            get => _speakerDuration;
            set
            {
                if (_speakerDuration != value)
                {
                    _speakerDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        private Color _textColor;
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

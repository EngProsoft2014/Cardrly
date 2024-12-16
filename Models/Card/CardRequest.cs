using System.ComponentModel;

namespace Cardrly.Mode_s.Card
{
    public class CardRequest : INotifyPropertyChanged
    {
        public string CardName { get; set; } = default!;
        public string? Cardlayout { get; set; } = default!;
        public byte[]? ImgFileProfile { get; set; }
        ImageSource? _ImgProfileFile;
        public ImageSource? ImgProfileFile
        {
            get
            {
                return _ImgProfileFile;
            }
            set
            {
                _ImgProfileFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ImgProfileFile"));
                }
            }
        }
        public string? ExtensionProfile { get; set; } = "No";
        public byte[]? ImgFileCover { get; set; }
        ImageSource? _ImgCoverFile;
        public ImageSource? ImgCoverFile
        {
            get
            {
                return _ImgCoverFile;
            }
            set
            {
                _ImgCoverFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ImgCoverFile"));
                }
            }
        }
        public string? ExtensionCover { get; set; } = "No";
        public string? PersonName { get; set; } = default!;
        public string? PersonNikeName { get; set; } = default!;
        public string? location { get; set; } = default!;
        public string? JobTitle { get; set; } = default!;
        public string? Bio { get; set; } = default!;
        public string? CardTheme { get; set; } = default!;
        public string? LinkColor { get; set; } = default!;
        public string? FontStyle { get; set; } = default!;


        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

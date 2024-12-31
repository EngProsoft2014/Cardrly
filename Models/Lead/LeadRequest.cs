using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cardrly.Models.Lead
{
    public class LeadRequest : INotifyPropertyChanged
    {
        private string _fullName = default!;
        private string? _email = default!;
        private string? _address = default!;
        private string? _phone = default!;
        private string? _company = default!;
        private string? _website = default!;
        private string? _JobTitle = default!;
        private string? _PersonName = default!;
        private string? _UrlImgProfile = default!;
        private byte[]? _imgFile;
        private string? _extension = string.Empty;

        public string? CardId { get; set; }
        public string? LeadCategoryId { get; set; }
        public string PersonName
        {
            get => _PersonName;
            set => SetProperty(ref _PersonName, value);
        }
        public string UrlImgProfile
        {
            get => _UrlImgProfile;
            set => SetProperty(ref _UrlImgProfile, value);
        }
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }
        public string? Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }
        public string? Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }
        public string? Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }
        public string? Website
        {
            get => _website;
            set => SetProperty(ref _website, value);
        }
        public string? JobTitle
        {
            get => _JobTitle;
            set => SetProperty(ref _JobTitle, value);
        }
        public byte[]? ImgFile
        {
            get => _imgFile;
            set => SetProperty(ref _imgFile, value);
        }
        public string? Extension
        {
            get => _extension;
            set => SetProperty(ref _extension, value);
        }
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

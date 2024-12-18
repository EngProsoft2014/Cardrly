

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cardrly.Mode_s.CardLink
{
    public class CardLinkResponse : INotifyPropertyChanged
    {
        public string Id { get; set; } = default!;
        public string CardId { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public string AccountLinkId { get; set; } = default!;
        public string AccountLinkTitle { get; set; } = default!;
        public int CardLinkType { get; set; } = default!;
        public string? AccountLinkUrlImgName { get; set; } = default!;
        private string? _ValueOf;
        public string ValueOf
        {
            get => _ValueOf;
            set
            {
                if (_ValueOf != value)
                {
                    _ValueOf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ValueOf")); // reports this property
                }
            }
        }
        public bool? Active { get; set; } = default!;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Cardrly.Mode_s.Account
{
    public class AccountResponse : INotifyPropertyChanged
    {
        private double? _usersProgress;
        private double? _cardProgress;
        private double? _expireProgress;
        private int? _remmingDays;
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Logo { get; set; }
        public string? UrlLogo { get; set; }
        public DateOnly? StartDateAcc { get; set; }
        public DateOnly? ExpireDateAcc { get; set; }
        public int DayOperationAcc { get; set; }
        public int DayOperationExpireAcc { get; set; }
        public int? CountUsers { get; set; }
        public int? CountCards { get; set; }
        public int? CurrentCountUsers { get; set; }
        public int? CurrentCountCards { get; set; }
        public double? UsersProgress
        {
            get => _usersProgress;
            set
            {
                if (_usersProgress != value)
                {
                    _usersProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        public double? CardProgress
        {
            get => _cardProgress;
            set
            {
                if (_cardProgress != value)
                {
                    _cardProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        public double? ExpireProgress
        {
            get => _expireProgress;
            set
            {
                if (_expireProgress != value)
                {
                    _expireProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        public int? RemmingDays
        {
            get => _remmingDays;
            set
            {
                if (_remmingDays != value)
                {
                    _remmingDays = value;
                    OnPropertyChanged();
                }
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

using Cardrly.Constants;
using Cardrly.Enums;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Models.Calendar;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using System.Collections.ObjectModel;
using static Cardrly.Models.Calendar.CalendlyResponseModel;
using static Cardrly.Models.Calendar.GmailResponseModel;
using static Cardrly.Models.Calendar.OutLookResponseModel;

namespace Cardrly.ViewModels
{
    public partial class CalendarViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        public int isPassed = 0; //0 => out coming - 1 => passed 
        [ObservableProperty]
        public ObservableCollection<CardResponse> cardLst = new ObservableCollection<CardResponse>();
        [ObservableProperty]
        public ObservableCollection<CalendarTypeItemModel> calendarTypes = new ObservableCollection<CalendarTypeItemModel>();
        [ObservableProperty]
        CardResponse selectedCard = new CardResponse();
        [ObservableProperty]
        public CalendarTypeItemModel selectedProvider = new CalendarTypeItemModel();
        [ObservableProperty]
        DateTime fromDate = DateTime.UtcNow.Date;
        [ObservableProperty]
        DateTime toDate = DateTime.UtcNow.Date;
        [ObservableProperty]
        public ObservableCollection<CalendarCalendlyResponse> calendlyResponses = new ObservableCollection<CalendarCalendlyResponse>();
        [ObservableProperty]
        public ObservableCollection<CalendarEventGmail> calendarEventGmails = new ObservableCollection<CalendarEventGmail>();
        [ObservableProperty]
        public ObservableCollection<CalendarOutlookEvent> calendarOutlookEvents = new ObservableCollection<CalendarOutlookEvent>();
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public CalendarViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        public async Task GetData()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                UserDialogs.Instance.ShowLoading();
                if (SelectedProvider.Value == 3)
                {
                    await GetCalendlyData(UserToken);
                }
                else if (SelectedProvider.Value == 2)
                {
                    await GetOutLookData(UserToken);
                }
                else if (SelectedProvider.Value == 1)
                {
                    await GetGmailData(UserToken);
                }
                UserDialogs.Instance.HideHud();
            }
            IsEnable = true;
        }
        #endregion

        #region Methods
        public async void Init()
        {
            IsEnable = false;
            UserDialogs.Instance.ShowLoading();
            await GetAllCards();
            if (CardLst.Count > 0)
            {
                SelectedCard = CardLst[0];
            }
            CalendarTypes.Add(new CalendarTypeItemModel() { Name = EnumCalendarType.Calendly.ToString(), Value = (int)EnumCalendarType.Calendly });
            CalendarTypes.Add(new CalendarTypeItemModel() { Name = EnumCalendarType.Gmail.ToString(), Value = (int)EnumCalendarType.Gmail });
            CalendarTypes.Add(new CalendarTypeItemModel() { Name = EnumCalendarType.Outlook.ToString(), Value = (int)EnumCalendarType.Outlook });
            SelectedProvider = CalendarTypes.FirstOrDefault()!;
            UserDialogs.Instance.HideHud();
            IsEnable = true;
        }
        public async Task GetAllCards()
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

                var json = await Rep.GetAsync<ObservableCollection<CardResponse>>($"{ApiConstants.CardGetAllApi}{AccId}/Card", UserToken);

                if (json != null && json.Count > 0)
                {
                    CardLst = json;
                }
            }
        }
        public async Task GetCalendlyData(string UserToken)
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

            var json = await Rep.GetAsync<ObservableCollection<CalendarCalendlyResponse>>($"{ApiConstants.CalnderGetByApi}{AccId}/Calendar/CalendarType/{SelectedProvider.Value}/{FromDate.Date.ToString("yyyy-MM-dd")}/{ToDate.Date.ToString("yyyy-MM-dd")}?cardid={SelectedCard.Id}", UserToken);

            if (json != null && json.Count > 0)
            {
                CalendlyResponses = json;
            }
        }
        public async Task GetGmailData(string UserToken)
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

            var json = await Rep.GetAsync<ObservableCollection<CalendarGmailResponse>>($"{ApiConstants.CalnderGetByApi}{AccId}/Calendar/CalendarType/{SelectedProvider.Value}/{FromDate.Date.ToString("yyyy-MM-dd")}/{ToDate.Date.ToString("yyyy-MM-dd")}?cardid={SelectedCard.Id}", UserToken);

            if (json != null && json.Count > 0)
            {
                CalendarEventGmails = new ObservableCollection<CalendarEventGmail>(json.FirstOrDefault()!.Items);
            }
        }
        public async Task GetOutLookData(string UserToken)
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

            var json = await Rep.GetAsync<ObservableCollection<CalendarOutLookResponse>>($"{ApiConstants.CalnderGetByApi}{AccId}/Calendar/CalendarType/{SelectedProvider.Value}/{FromDate.Date.ToString("yyyy-MM-dd")}/{ToDate.Date.ToString("yyyy-MM-dd")}?cardid={SelectedCard.Id}", UserToken);

            if (json != null && json.Count > 0)
            {
                CalendarOutlookEvents = new ObservableCollection<CalendarOutlookEvent>(json.FirstOrDefault()!.Events);
            }
        }
        #endregion
    }
}

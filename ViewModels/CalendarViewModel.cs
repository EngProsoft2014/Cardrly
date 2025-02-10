using Cardrly.Constants;
using Cardrly.Enums;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Models.Calendar;
using Cardrly.Pages.MainPopups;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
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
        public ObservableCollection<CardResponse> cardLst = new ObservableCollection<CardResponse>();
        [ObservableProperty]
        public ObservableCollection<CalendarTypeItemModel> calendarTypes = new ObservableCollection<CalendarTypeItemModel>();
        [ObservableProperty]
        CardResponse selectedCard = new CardResponse();
        [ObservableProperty]
        public CalendarTypeItemModel selectedProvider = new CalendarTypeItemModel();
        [ObservableProperty]
        string fromDate;
        [ObservableProperty]
        string toDate;
        [ObservableProperty]
        bool isShowCollection = false;
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
        public async Task GetAllDataClick()
        {

            //if (FromDate < ToDate)
            //{
            //    await GetData();
            //}
            //else
            //{
            //    var toast = Toast.Make($"{AppResources.msgDate_HomePage}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            //    await toast.Show();
            //}
        }
        [RelayCommand]
        public async Task CalendlyItemClick(object item)
        {
            if (item is CalendarCalendlyResponse)
            {
                await MopupService.Instance.PushAsync(new CalendlyDetailsPopup((CalendarCalendlyResponse)item));
            }
            else if (item is CalendarEventGmail)
            {
                await MopupService.Instance.PushAsync(new GmailDetailsPopup((CalendarEventGmail)item));
            }
            else if (item is CalendarOutlookEvent)
            {
                await MopupService.Instance.PushAsync(new OutLookDetailsPopup((CalendarOutlookEvent)item));
            }

        }
        [RelayCommand]
        public async Task FilterClick()
        {
            var page = new CalendrFilterPopup(CalendarTypes, CardLst);
            page.FilterClose += async (from, To, CalenderType, Card) =>
            {
                FromDate = from;
                ToDate = To;
                SelectedProvider = CalenderType;
                SelectedCard = Card;
                await GetData();
            };
            await MopupService.Instance.PushAsync(page);
        }
        #endregion

        #region Methods
        public async void Init()
        {
            IsEnable = false;
            UserDialogs.Instance.ShowLoading();
            await GetAllCards();
            CalendarTypes.Add(new CalendarTypeItemModel() { Name = EnumCalendarType.Calendly.ToString(), Value = (int)EnumCalendarType.Calendly });
            CalendarTypes.Add(new CalendarTypeItemModel() { Name = EnumCalendarType.Gmail.ToString(), Value = (int)EnumCalendarType.Gmail });
            CalendarTypes.Add(new CalendarTypeItemModel() { Name = EnumCalendarType.Outlook.ToString(), Value = (int)EnumCalendarType.Outlook });
            //await UpComingClick();
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
        public async Task GetCalendlyData()
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                var json = await Rep.GetAsync<ObservableCollection<CalendarCalendlyResponse>>($"{ApiConstants.CalnderGetByApi}{AccId}/Calendar/CalendarType/{SelectedProvider.Value}/{FromDate}/{ToDate}?cardid={SelectedCard.Id}", UserToken);

                if (json != null)
                {
                    CalendlyResponses = json;
                }
            }
        }
        public async Task GetGmailData()
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                var json = await Rep.GetAsync<ObservableCollection<CalendarGmailResponse>>($"{ApiConstants.CalnderGetByApi}{AccId}/Calendar/CalendarType/{SelectedProvider.Value}/{FromDate}/{ToDate}?cardid={SelectedCard.Id}", UserToken);

                if (json != null)
                {
                    CalendarEventGmails = new ObservableCollection<CalendarEventGmail>(json.FirstOrDefault()!.Items);
                }
            }
        }
        public async Task GetOutLookData()
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                var json = await Rep.GetAsync<ObservableCollection<CalendarOutLookResponse>>($"{ApiConstants.CalnderGetByApi}{AccId}/Calendar/CalendarType/{SelectedProvider.Value}/{FromDate}/{ToDate}?cardid={SelectedCard.Id}", UserToken);

                if (json != null)
                {
                    CalendarOutlookEvents = new ObservableCollection<CalendarOutlookEvent>(json.FirstOrDefault()!.Events);
                }
            }
        }
        public async Task GetData()
        {
            IsEnable = false;
            CalendlyResponses = new ObservableCollection<CalendarCalendlyResponse>();
            CalendarOutlookEvents = new ObservableCollection<CalendarOutlookEvent>();
            CalendarEventGmails = new ObservableCollection<CalendarEventGmail>();
            UserDialogs.Instance.ShowLoading();
            if (SelectedProvider.Value == 3)
            {
                await GetCalendlyData();
            }
            else if (SelectedProvider.Value == 2)
            {
                await GetOutLookData();
            }
            else if (SelectedProvider.Value == 1)
            {
                await GetGmailData();
            }

            if(CalendlyResponses.Count > 0 || CalendarOutlookEvents.Count > 0 || CalendarEventGmails.Count > 0)
            {
                IsShowCollection = true;
            }
            else
            {
                IsShowCollection = false;
            }
            UserDialogs.Instance.HideHud();
            IsEnable = true;
        }
        #endregion
    }
}

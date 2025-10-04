using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Card;
using Cardrly.Models.Calendar;
using Cardrly.Pages;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class AddEventViewModel : BaseViewModel
    {
        #region Properties
        [ObservableProperty]
        CalendarGmailRequest request = new CalendarGmailRequest();
        [ObservableProperty]
        TimeSpan startTime = DateTime.Now.AddHours(1).TimeOfDay;
        [ObservableProperty]
        TimeSpan endTime = DateTime.Now.AddHours(2).TimeOfDay;
        [ObservableProperty]
        CardResponse selectedCard = new CardResponse();
        [ObservableProperty]
        CalendarTypeItemModel selectedCalendarType = new CalendarTypeItemModel();
        [ObservableProperty]
        public ObservableCollection<CardResponse> cardLst = new ObservableCollection<CardResponse>();
        [ObservableProperty]
        bool isFoundCards = false;
        [ObservableProperty]
        ObservableCollection<CalendarTypeItemModel> calendarTypes = new ObservableCollection<CalendarTypeItemModel>();
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public AddEventViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service , ObservableCollection<CalendarTypeItemModel> calTypes)
        {
            Rep = GenericRep;
            _service = service;
            CalendarTypes = calTypes;
            SelectedCalendarType = calTypes[1];//Gmail
            Init();
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        public async Task SaveClick(CalendarGmailRequest gmailRequest)
        {
            IsEnable = false;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                DateTime StartDate = Request.Start + StartTime;
                DateTime EndDate = Request.End + EndTime;

                string valid = "";
                if (!string.IsNullOrEmpty(Request.Attendees))
                {
                    valid = CheckStringType(Request.Attendees);
                }
                if (SelectedCalendarType.Value == 0)
                {
                    var toast = Toast.Make($"{AppResources.msgPlease_select_a_provider}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (string.IsNullOrEmpty(SelectedCard.Id))
                {
                    var toast = Toast.Make($"{AppResources.msgPlease_select_a_Card}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (DateTime.Compare(Request.End, Request.Start) < 0)
                {
                    var toast = Toast.Make($"{AppResources.msgStart_date_must_be_before_end_date}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (DateTime.Compare(Request.Start, DateTime.UtcNow.Date) < 0)
                {
                    var toast = Toast.Make($"{AppResources.msgStart_date_must_be_from_todays_date}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if ((DateTime.Compare(Request.End, Request.Start) == 0) && (StartTime > EndTime))
                {
                    var toast = Toast.Make($"{AppResources.msgStart_time_must_be_before_end_time}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (string.IsNullOrEmpty(Request.TimeZone))
                {
                    var toast = Toast.Make($"{AppResources.msgFRTimeZone}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (string.IsNullOrEmpty(Request.Summary))
                {
                    var toast = Toast.Make($"{AppResources.msgFRSummary}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (string.IsNullOrEmpty(Request.Location))
                {
                    var toast = Toast.Make($"{AppResources.msgFRLocation}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (!string.IsNullOrEmpty(Request.Attendees) && valid != "Email")
                {
                    var toast = Toast.Make($"{AppResources.msgAttendance_must_be_in_email_format}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else if (string.IsNullOrEmpty(Request.Description))
                {
                    var toast = Toast.Make($"{AppResources.msgFRDescription}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else
                {
                    string UserToken = await _service.UserToken();
                    string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                    Request.Start = StartDate;
                    Request.End = EndDate;
                    UserDialogs.Instance.ShowLoading();
                    var json = await Rep.PostStrErrorAsync<CalendarGmailRequest>($"{ApiConstants.CalendarAddEventsApi}{accid}/Calendar/CalendarType/{SelectedCalendarType.Value}/AddEvents?CardId={SelectedCard.Id}", Request, UserToken);
                    UserDialogs.Instance.HideHud();

                    if (json.Item1 != null && json.Item1 == "")
                    {
                        var toast = Toast.Make($"{AppResources.msgSuccessfullyAddEvent}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                        MessagingCenter.Send(this, "AddEvent", SelectedCard);
                        await App.Current!.MainPage!.Navigation.PopAsync();
                    }
                    else
                    {
                        Request.Start = StartDate.Date;
                        Request.End = EndDate.Date;
                        var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Value.ToString()!.Replace("[", "").Replace("]", "").Replace("\"", "")}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
            }
            IsEnable = true;
        }
        [RelayCommand]
        public async Task TimeZoneClick()
        {
            IsEnable = false;
            await App.Current!.MainPage!.Navigation.PushAsync(new TimeZonePage(new TimeZoneViewModel()));
            IsEnable = true;
        }
        #endregion

        #region Methods
        public async void Init()
        {
            IsEnable = false;
            SelectedCard = new CardResponse() { CardName = "NoCard" };
            UserDialogs.Instance.ShowLoading();
            await GetAllCards();
            if (CardLst.Count > 0)
            {
                //SelectedCard = CardLst[0];
                IsFoundCards = true;
            }
            UserDialogs.Instance.HideHud();
            IsEnable = true;
            MessagingCenter.Subscribe<TimeZoneViewModel, string>(this, "TimeZoneSelected", async (sender, message) =>
            {
                if (message != null)
                {
                    Request.TimeZone = message;
                }
            });
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

        public string CheckStringType(string input)
        {
            // Email pattern
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // URL pattern
            string urlPattern = @"^(http|https)://[^\s/$.?#].[^\s]*$";

            // Phone number pattern (international format: +123456789 or local 123-456-7890)
            string phonePattern = @"^(\+?\d{1,3})?[-.\s]?\(?\d{2,4}\)?[-.\s]?\d{3}[-.\s]?\d{3,4}$";
            // Any string pattern
            string pattern = @"^[\s\S]+$"; // Matches any plain text, including new lines

            if (Regex.IsMatch(input, emailPattern))
            {
                return "Email";
            }
            else if (Regex.IsMatch(input, urlPattern))
            {
                return "URL";
            }
            else if (Regex.IsMatch(input, phonePattern))
            {
                return "Phone Number";
            }
            else if (Regex.IsMatch(input, pattern))
            {
                return "Text";
            }
            else
            {
                return "Unknown";
            }
        }
        #endregion
    }
}

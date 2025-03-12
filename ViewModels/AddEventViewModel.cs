using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
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
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class AddEventViewModel : BaseViewModel
    {
        #region Properties
        [ObservableProperty]
        CalendarGmailRequest request = new CalendarGmailRequest();
        [ObservableProperty]
        TimeSpan startTime = DateTime.Now.TimeOfDay;
        [ObservableProperty]
        TimeSpan endTime = DateTime.Now.AddHours(1).TimeOfDay;
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
            //SelectedCalendarType = calTypes[0];
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
                Request.Start = Request.Start + StartTime;
                Request.End = Request.End + EndTime;
                if (Request.Start > Request.End)
                {
                    var toast = Toast.Make($"{AppResources.msgErrorinstartandenddate}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
                else if (string.IsNullOrEmpty(Request.Description))
                {
                    var toast = Toast.Make($"{AppResources.msgFRDescription}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else
                {
                    string UserToken = await _service.UserToken();
                    string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                    UserDialogs.Instance.ShowLoading();
                    string json = await Rep.PostStrAsync<CalendarGmailRequest>($"{ApiConstants.CalendarAddEventsApi}{accid}/Calendar/CalendarType/{SelectedCalendarType.Value}/AddEvents?CardId={SelectedCard.Id}", Request, UserToken);
                    UserDialogs.Instance.HideHud();
                    if (json == "")
                    {
                        MessagingCenter.Send(this, "AddEvent", SelectedCard);
                        await App.Current!.MainPage!.Navigation.PopAsync();
                    }
                    else
                    {
                        var toast = Toast.Make(json, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
            }
        }
        [RelayCommand]
        public async Task TimeZoneClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new TimeZonePage(new TimeZoneViewModel()));
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
        #endregion
    }
}

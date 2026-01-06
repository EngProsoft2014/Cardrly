
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.TimeSheet;
using Cardrly.Pages.MainPopups;
using Cardrly.Pages.TrackingPages;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;


namespace Cardrly.ViewModels
{
    public partial class TimeSheetViewModel : BaseViewModel
    {
        #region Service
        readonly IGenericRepository ORep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Prop
        [ObservableProperty]
        ObservableCollection<TimeSheetResponse> lstEmployeesIn = new ObservableCollection<TimeSheetResponse>();

        [ObservableProperty]
        ObservableCollection<TimeSheetResponse> lstEmployeesOut = new ObservableCollection<TimeSheetResponse>();

        [ObservableProperty]
        bool isRefresh;

        [ObservableProperty]
        string date;

        [ObservableProperty]
        string numIn;

        [ObservableProperty]
        bool isShowNoDataIn;

        [ObservableProperty]
        bool isShowNoDataOut;

        [ObservableProperty]
        string numOut;

        [ObservableProperty]
        bool isShowBaseCheckIn = true;

        [ObservableProperty]
        bool isShowBaseCheckOut;

        [ObservableProperty]
        bool isShowBaseBreakIn;

        [ObservableProperty]
        bool isShowBaseBreakOut;

        [ObservableProperty]
        bool isShowMyCheckIn;

        [ObservableProperty]
        bool isShowMyCheckOut;

        [ObservableProperty]
        bool isShowMyBreakIn;

        [ObservableProperty]
        bool isShowMyBreakOut;

        DateTime dateDF;
        #endregion

        #region Cons
        public TimeSheetViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            ORep = GenericRep;
            _service = service;
            Init();
            MessagingCenter.Subscribe<CheckoutPopup, bool>(this, "ChangeEmployeeTime", (sender, message) =>
            {
                if (true)
                {
                    Init();
                }
            });
        }
        #endregion

        #region Methods
        void Init()
        {
            if (Controls.StaticMember.SelectedDate.ToString("MM-dd-yyyy") == DateTime.Now.ToString("MM-dd-yyyy"))
            {
                dateDF = DateTime.Now;
                Date = AppResources.lblToday;
            }
            else
            {
                dateDF = Controls.StaticMember.SelectedDate;
                Date = Controls.StaticMember.SelectedDate.ToString("MM-dd-yyyy");
            }


            //if(LstEmployeesIn.Count == 0)
            //{
            //    CheckInOutModel oCheckInOutModel = new CheckInOutModel
            //    {
            //        Id = 1,
            //        EmployeeName = Preferences.Default.Get(ApiConstants.username, ""),
            //        EmployeeId = int.TryParse(Preferences.Default.Get(ApiConstants.userid, "0"), out var empId) ? empId : 0,
            //        AccountId = Preferences.Default.Get(ApiConstants.AccountId, ""),
            //        Date = dateDF.ToString("yyyy-MM-dd"),
            //        CreateDate = DateTime.Now,
            //        CreateUser = int.TryParse(Preferences.Default.Get(ApiConstants.userid, "0"), out var createUser) ? createUser : 0,
            //        SheetColor = "#26cc8a",
            //        Active = true,
            //        HoursFrom = "",
            //        HoursTo = "",
            //        DurationHours = "",
            //        DurationMinutes = "",
            //        Notes = "",
            //    };

            //    LstEmployeesIn.Add(oCheckInOutModel);
            //    LstEmployeesOut.Add(oCheckInOutModel);
            //    NumIn = LstEmployeesIn.Count.ToString();
            //    NumOut = LstEmployeesOut.Count.ToString();
            //}

            GetCheckInOutEmployees(dateDF.ToString("MM-dd-yyyy"));

        }

        public async void GetCheckInOutEmployees(string date)
        {

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                UserDialogs.Instance.ShowLoading();
                string UserToken = await _service.UserToken();

                string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string UserId = Preferences.Default.Get(ApiConstants.userid, "");

                var json = await ORep.GetAsync<List<TimeSheetResponse>>(ApiConstants.GetByDateTimeSheetApi + AccountId + "/" + UserId + "/" + date, UserToken);

                if (json != null)
                {
                    LstEmployeesIn = new ObservableCollection<TimeSheetResponse>(json.Where(x => x.HoursTo == null).OrderBy(x => x.CardName).ToList());
                    LstEmployeesOut = new ObservableCollection<TimeSheetResponse>(json.Where(x => x.HoursTo != null).OrderBy(x => x.CardName).ToList());

                    NumIn = LstEmployeesIn.Count.ToString();
                    NumOut = LstEmployeesOut.Count.ToString();
                }
                else
                {
                    IsShowNoDataIn = true;
                    IsShowNoDataOut = true;
                    NumIn = "0";
                    NumOut = "0";
                }

                UserDialogs.Instance.HideHud();
            }
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async void GoTracking(ObservableCollection<TimeSheetResponse> lstEmployeesIn)
        {
            UserDialogs.Instance.ShowLoading();

            ObservableCollection<TimeSheetResponse> lstEmployeesTracking = new ObservableCollection<TimeSheetResponse>(lstEmployeesIn.Where(x => x.HoursFrom != null && x.HoursTo == null).ToList());

            await App.Current!.MainPage!.Navigation.PushAsync(new EmployeesWorkingPage(new EmployeesViewModel(lstEmployeesTracking, ORep, _service), ORep, _service));
            UserDialogs.Instance.HideHud();
        }

        [RelayCommand]
        void RefreshLstEmployees()
        {
            IsRefresh = true;

            GetCheckInOutEmployees(dateDF.ToString("MM-dd-yyyy"));

            IsRefresh = false;
        }

        [RelayCommand]
        void NextDay()
        {
            IsEnable = false;
            dateDF = Controls.StaticMember.SelectedDate;
            Date = dateDF.AddDays(1).ToString("MM-dd-yyyy") == DateTime.Now.ToString("MM-dd-yyyy") ? AppResources.lblToday : dateDF.AddDays(1).ToString("MM-dd-yyyy");
            Controls.StaticMember.SelectedDate = dateDF = dateDF.AddDays(1);
            RefreshLstEmployees();
            IsEnable = true;
        }

        [RelayCommand]
        void BackDay()
        {
            IsEnable = false;
            dateDF = Controls.StaticMember.SelectedDate;
            Date = dateDF.AddDays(-1).ToString("MM-dd-yyyy") == DateTime.Now.ToString("MM-dd-yyyy") ? AppResources.lblToday : dateDF.AddDays(-1).ToString("MM-dd-yyyy");
            Controls.StaticMember.SelectedDate = dateDF = dateDF.AddDays(-1);
            RefreshLstEmployees();
            IsEnable = true;
        }

        //[RelayCommand]
        //async Task SelectedDate()
        //{
        //    IsEnable = false;
        //    var popupView = new Pages.MainPopups.DatePopup();
        //    popupView.RangeClose += (calendar) =>
        //    {

        //        GetCheckInOutEmployees(calendar.StartDate.Value.ToString("MM-dd-yyyy"));

        //        Date = calendar.StartDate.Value.ToString("MM-dd-yyyy");

        //    };

        //    await MopupService.Instance.PushAsync(popupView);
        //    IsEnable = true;
        //}

        //[RelayCommand]
        //async Task SelectedDate()
        //{
        //    var popup = new DatePopup();

        //    void Handler(CalendarModel calendar)
        //    {
        //        GetCheckInOutEmployees(calendar.StartDate.Value.ToString("MM-dd-yyyy"));
        //        Date = calendar.StartDate.Value.ToString("MM-dd-yyyy");

        //        popup.RangeClose -= Handler; // 🔥 مهم جدًا
        //    }

        //    popup.RangeClose += Handler;

        //    await MopupService.Instance.PushAsync(popup);
        //}

        [RelayCommand]
        async Task SelectedDate()
        {
            IsEnable = false;

            var tcs = new TaskCompletionSource<CalendarModel?>();
            var popup = new DatePopup(tcs);

            await MopupService.Instance.PushAsync(popup);

            var result = await tcs.Task;

            if (result != null)
            {
                Date = result.StartDate!.Value.ToString("MM-dd-yyyy");
                GetCheckInOutEmployees(Date);
            }

            IsEnable = true;
        }

        [RelayCommand]
        async Task SelectedCheckIn(TimeSheetResponse model)
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var popupView = new CheckoutPopup(model.Id, model.HoursFrom.ToString()!, this, ORep, _service);
                popupView.TimeDidClose += async (time) =>
                {
                    string UserToken = await _service.UserToken();
                    //model.HoursFrom = string.Format(time.ToString(@"hh\:mm"));
                    //model.HoursFrom = time;
                    //model.Active = true;

                    string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");

                    CreateTimeSheet obj = new CreateTimeSheet
                    {
                        CardId = model.CardId,
                        HoursFrom = time,
                        TimeSheetBranchId = model.TimeSheetBranchId,
                        WorkDate = DateTime.Now,
                        //CheckinAddress = model.CheckinAddress,
                        CheckinAddress = "15 el salam st Alexandria, Egypt",
                    };

                    var json = await ORep.PostTRAsync<CreateTimeSheet, TimeSheetResponse>(ApiConstants.AddTimeSheetApi + AccountId, obj, UserToken);

                    if (json.Item1 != null)
                    {
                        Init();
                        IsShowBaseCheckIn = false;
                        IsShowBaseCheckOut = true;
                        IsShowBaseBreakIn = true;
                        IsShowBaseBreakOut = false;

                        var toast = Toast.Make(AppResources.msgSuccessfullyCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                    else
                    {
                        var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value.ToString()!.Replace('[',' ').Replace(']',' ')}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                };

                await MopupService.Instance.PushAsync(popupView);
            }

            IsEnable = true;
        }

        [RelayCommand]
        async Task SelectedTimeOut(TimeSheetResponse model)
        {
            IsEnable = false;

            //if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            //{
            //    var popupView = new CheckoutPopup(model.HoursFrom!,new TimeSheetViewModel(ORep,_service), ORep,_service);
            //    popupView.TimeDidClose += async (time) =>
            //    {
            //        if (time > TimeSpan.Parse(model.HoursFrom!))
            //        {
            //            string UserToken = await _service.UserToken();
            //            //model.HoursTo = string.Format("{0:hh:mm}", time.ToString());
            //            model.HoursTo = string.Format(time.ToString(@"hh\:mm"));
            //            model.DurationHours = (time - TimeSpan.Parse(model.HoursFrom!)).Hours.ToString();
            //            model.DurationMinutes = (time - TimeSpan.Parse(model.HoursFrom!)).Minutes.ToString();

            //            await ORep.PutAsync(string.Format("api/TimeSheet/PutCheckInOut/{0}", model.EmployeeId), model, UserToken);
            //        }
            //        else
            //        {
            //            var toast = Toast.Make(AppResources.msgPleaseChooseTimeAfterCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            //            await toast.Show();
            //        }
            //    };

            //    await MopupService.Instance.PushAsync(popupView);
            //}

            IsEnable = true;
        }

        [RelayCommand]
        void SelectedBaseBreakIn(TimeSheetResponse model)
        {
            IsShowBaseCheckIn = false;
            IsShowBaseCheckOut = true;
            IsShowBaseBreakIn = false;
            IsShowBaseBreakOut = true;
        }

        [RelayCommand]
        void SelectedBaseBreakOut(TimeSheetResponse model)
        {
            IsShowBaseCheckIn = true;
            IsShowBaseCheckOut = true;
            IsShowBaseBreakIn = false;
            IsShowBaseBreakOut = false;
        }

        [RelayCommand]
        async Task SelectedTimeMyStart(TimeSheetResponse model)
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                UserDialogs.Instance.ShowLoading();

            //    string UserToken = await _service.UserToken();

            //    model.HoursFrom = string.Format(DateTime.Now.ToString(@"hh\:mm"));
            //    model.Date = Controls.StaticMember.SelectedDate.ToString("yyyy-MM-dd");
            //    model.CreateDate = DateTime.Now;
            //    model.CreateUser = Preferences.Default.Get(ApiConstants.userid, 0);
            //    model.SheetColor = "#26cc8a";
            //    model.Active = true;

            //    var json = await ORep.PostAsync("api/TimeSheet/PostCheckInOut", model, UserToken);

            //    if (json != null && json.EmployeeId != 0 && json.EmployeeId != null)
            //    {
            //        Init();
            //        var toast = Toast.Make(AppResources.msgSuccessfullyCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            //        await toast.Show();
            //    }
            //    else
            //    {
            //        var toast = Toast.Make(AppResources.msgFailedCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            //        await toast.Show();
            //    }

            //    UserDialogs.Instance.HideHud();
            //}

            IsEnable = true;
        }

        [RelayCommand]
        async Task SelectedTimeMyEnd(TimeSheetResponse model)
        {
            IsEnable = false;

            //if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            //{
            //    UserDialogs.Instance.ShowLoading();

            //    TimeSpan time = DateTime.Now.TimeOfDay;

            //    if (time > TimeSpan.Parse(model.HoursFrom!))
            //    {
            //        string UserToken = await _service.UserToken();

            //        model.HoursTo = string.Format(time.ToString(@"hh\:mm"));
            //        model.DurationHours = (time - TimeSpan.Parse(model.HoursFrom!)).Hours.ToString();
            //        model.DurationMinutes = (time - TimeSpan.Parse(model.HoursFrom!)).Minutes.ToString();

            //        var json = await ORep.PutAsync(string.Format("api/TimeSheet/PutCheckInOut/{0}", model.EmployeeId), model, UserToken);

            //        if (json != null)
            //        {
            //            Init();
            //            var toast = Toast.Make(AppResources.msgSuccessfullyCheckOutTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            //            await toast.Show();
            //        }
            //    }
            //    else
            //    {
            //        var toast = Toast.Make(AppResources.msgPleaseChooseTimeAfterCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            //        await toast.Show();
            //    }

                UserDialogs.Instance.HideHud();
            }

            IsEnable = true;
        }

        [RelayCommand]
        void SelectedMyBrackIn(TimeSheetResponse model)
        {

        }

        [RelayCommand]
        void SelectedMyBreakOut(TimeSheetResponse model)
        {

        }
        #endregion
    }
}

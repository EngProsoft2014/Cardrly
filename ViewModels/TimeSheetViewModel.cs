
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Pages.MainPopups;
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
        ObservableCollection<CheckInOutModel> lstEmployeesIn;

        [ObservableProperty]
        ObservableCollection<CheckInOutModel> lstEmployeesOut;

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

            LstEmployeesIn = new ObservableCollection<CheckInOutModel>();
            LstEmployeesOut = new ObservableCollection<CheckInOutModel>();

            GetCheckInOutEmployees(dateDF.ToString("MM-dd-yyyy"));
        }

        public async void GetCheckInOutEmployees(string date)
        {
            //if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            //{
            //    UserDialogs.Instance.ShowLoading();
            //    string UserToken = await _service.UserToken();
            //    var json = await ORep.GetAsync<List<CheckInOutModel>>("api/TimeSheet/GetCheckInOut?" + "date=" + date + "&" + "userId=" + Settings.UserIdGet + "&" + "userRole=" + Controls.StartData.EmployeeDataStatic.UserRole.ToString(), UserToken);

            //    if (json != null)
            //    {
            //        LstEmployeesIn = new ObservableCollection<CheckInOutModel>(json.Where(x => x.HoursTo == "" || x.HoursTo == null).OrderBy(x => x.EmployeeName).ToList());
            //        LstEmployeesOut = new ObservableCollection<CheckInOutModel>(json.Where(x => x.HoursTo != "" && x.HoursTo != null).OrderBy(x => x.EmployeeName).ToList());

            //        NumIn = LstEmployeesIn.Count.ToString();
            //        NumOut = LstEmployeesOut.Count.ToString();
            //    }
            //    else
            //    {
            //        IsShowNoDataIn = true;
            //        IsShowNoDataOut = true;
            //        NumIn = "0";
            //        NumOut = "0";
            //    }

            //    UserDialogs.Instance.HideHud();
            //}
        }
        #endregion

        #region RelayCommand
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

        [RelayCommand]
        async Task SelectedDate()
        {
            IsEnable = false;
            var popupView = new Pages.MainPopups.DatePopup();
            popupView.RangeClose += (calendar) =>
            {

                GetCheckInOutEmployees(calendar.StartDate.Value.ToString("MM-dd-yyyy"));

                Date = calendar.StartDate.Value.ToString("MM-dd-yyyy");

            };

            await MopupService.Instance.PushAsync(popupView);
            IsEnable = true;
        }

        [RelayCommand]
        async Task SelectedTimeIn(CheckInOutModel model)
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var popupView = new CheckoutPopup(model.Id, model.HoursFrom, new TimeSheetViewModel(ORep, _service), ORep, _service);
                popupView.TimeDidClose += async (time) =>
                {
                    string UserToken = await _service.UserToken();
                    model.HoursFrom = string.Format(time.ToString(@"hh\:mm"));
                    model.Active = true;

                    var json = await ORep.PutAsync(string.Format("api/TimeSheet/PutCheckInOut/{0}", model.EmployeeId), model, UserToken);

                    if (json != null)
                    {
                        Init();
                        var toast = Toast.Make(AppResources.msgSuccessfullyCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                };

                await MopupService.Instance.PushAsync(popupView);
            }

            IsEnable = true;
        }

        [RelayCommand]
        async Task SelectedTimeOut(CheckInOutModel model)
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var popupView = new CheckoutPopup(model.HoursFrom,new TimeSheetViewModel(ORep,_service), ORep,_service);
                popupView.TimeDidClose += async (time) =>
                {
                    if (time > TimeSpan.Parse(model.HoursFrom))
                    {
                        string UserToken = await _service.UserToken();
                        //model.HoursTo = string.Format("{0:hh:mm}", time.ToString());
                        model.HoursTo = string.Format(time.ToString(@"hh\:mm"));
                        model.DurationHours = (time - TimeSpan.Parse(model.HoursFrom)).Hours.ToString();
                        model.DurationMinutes = (time - TimeSpan.Parse(model.HoursFrom)).Minutes.ToString();

                        await ORep.PutAsync(string.Format("api/TimeSheet/PutCheckInOut/{0}", model.EmployeeId), model, UserToken);
                    }
                    else
                    {
                        var toast = Toast.Make(AppResources.msgPleaseChooseTimeAfterCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                };

                await MopupService.Instance.PushAsync(popupView);
            }

            IsEnable = true;
        }

        [RelayCommand]
        async Task SelectedTimeMyStart(CheckInOutModel model)
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                UserDialogs.Instance.ShowLoading();

                string UserToken = await _service.UserToken();

                model.HoursFrom = string.Format(DateTime.Now.ToString(@"hh\:mm"));
                model.Date = Controls.StaticMember.SelectedDate.ToString("yyyy-MM-dd");
                model.CreateDate = DateTime.Now;
                model.CreateUser = Preferences.Default.Get(ApiConstants.userid, 0);
                model.SheetColor = "#26cc8a";
                model.Active = true;

                var json = await ORep.PostAsync("api/TimeSheet/PostCheckInOut", model, UserToken);

                if (json != null && json.EmployeeId != 0 && json.EmployeeId != null)
                {
                    Init();
                    var toast = Toast.Make(AppResources.msgSuccessfullyCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else
                {
                    var toast = Toast.Make(AppResources.msgFailedCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }

                UserDialogs.Instance.HideHud();
            }

            IsEnable = true;
        }

        [RelayCommand]
        async Task SelectedTimeMyEnd(CheckInOutModel model)
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                UserDialogs.Instance.ShowLoading();

                TimeSpan time = DateTime.Now.TimeOfDay;

                if (time > TimeSpan.Parse(model.HoursFrom))
                {
                    string UserToken = await _service.UserToken();

                    model.HoursTo = string.Format(time.ToString(@"hh\:mm"));
                    model.DurationHours = (time - TimeSpan.Parse(model.HoursFrom)).Hours.ToString();
                    model.DurationMinutes = (time - TimeSpan.Parse(model.HoursFrom)).Minutes.ToString();

                    var json = await ORep.PutAsync(string.Format("api/TimeSheet/PutCheckInOut/{0}", model.EmployeeId), model, UserToken);

                    if (json != null)
                    {
                        Init();
                        var toast = Toast.Make(AppResources.msgSuccessfullyCheckOutTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                else
                {
                    var toast = Toast.Make(AppResources.msgPleaseChooseTimeAfterCheckInTime, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }

                UserDialogs.Instance.HideHud();
            }

            IsEnable = true;
        } 
        #endregion
    }
}

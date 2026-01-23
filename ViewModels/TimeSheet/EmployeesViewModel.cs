
using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.Card;
using Cardrly.Models.TimeSheet;
using Cardrly.Resources.Lan;
using Cardrly.Services.Data;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cardrly.ViewModels
{
    public partial class EmployeesViewModel : BaseViewModel
    {
        #region Service
        readonly IGenericRepository ORep;
        readonly Services.Data.ServicesService _service;
        readonly SignalRService _signalRService;
        #endregion

        #region Prop
        [ObservableProperty]
        ObservableCollection<TimeSheetResponse> lstWorkingEmployees;

        [ObservableProperty]
        ObservableCollection<DataMapsModel> listmap;

        [ObservableProperty]
        ObservableCollection<DataMapsModel> lastListmap;

        [ObservableProperty]
        TimeSheetResponse oneEmployee = new TimeSheetResponse();

        public static DataMapsModel MapsModel { get; set; }

        DataTable employeesTable;

        DataMapsModel CurrentTrack { get; set; }
        DataSet ds = new DataSet();
        XDocument document = new XDocument();
        #endregion

        #region Cons

        //Tracking Constructor
        public EmployeesViewModel(TimeSheetResponse employee, IGenericRepository GenericRep, ServicesService service)
        {
            ORep = GenericRep;
            _service = service;

            OneEmployee = employee;
            Listmap = new ObservableCollection<DataMapsModel>();
            LastListmap = new ObservableCollection<DataMapsModel>();
            CurrentTrack = new DataMapsModel();

            //new Timer((Object stateInfo) => { GetDataEmployee(); }, new AutoResetEvent(false), 0, 3000);
        }

        //Employees Working Today Constructor
        public EmployeesViewModel(ObservableCollection<TimeSheetResponse> lstEmployeesTracking, IGenericRepository GenericRep, ServicesService service, SignalRService signalRService)
        {
            ORep = GenericRep;
            _service = service;
            _signalRService = signalRService;
            LstWorkingEmployees = lstEmployeesTracking;
            //InitTraking(lstEmployeesTracking);
        }
        #endregion

        #region Methods
        //async void InitTraking(ObservableCollection<TimeSheetResponse> lstEmployeesTracking)
        //{
        //    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        //    {
        //        LstWorkingEmployees = new ObservableCollection<TimeSheetResponse>();

        //        string Date = DateTime.Now.ToString("yyyy-MM-dd");
        //        string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

        //        UserDialogs.Instance.ShowLoading();

        //        string UserToken = await _service.UserToken();
        //        //Get Working Employees
        //        var json = await ORep.GetAsync<ObservableCollection<TimeSheetResponse>>(ApiConstants.GetEmpWorkingTimeSheetApi + AccId + "/" + Date, UserToken);

        //        if (json != null)
        //        {
        //            LstWorkingEmployees = json;
        //        }

        //        UserDialogs.Instance.HideHud();
        //    }
        //}


        public void HandleLocationUpdate(DataMapsModel locationData)
        {
            //if (locationData.EmployeeId.ToString() == OneEmployee.Id)
            //{
            Device.BeginInvokeOnMainThread(() =>
            {
                MapsModel = locationData;
                // Update UI map pin here
            });
            //}
        }

        //public void HandleLocationUpdate(DataMapsModel locationData)
        //{
        //    if (locationData.EmployeeId.ToString() == OneEmployee.Id)
        //    {
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            // Update the UI with the latest location
        //            MapsModel = new DataMapsModel
        //            {
        //                Id = locationData.Id,
        //                EmployeeId = locationData.EmployeeId,
        //                Lat = locationData.Lat,
        //                Long = locationData.Long,
        //                Time = locationData.Time,
        //                CreateDate = locationData.CreateDate,
        //                MPosition = new Location(double.Parse(locationData.Lat), double.Parse(locationData.Long))
        //            };
        //        });
        //    }
        //}

        private async Task GetDataEmployee()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {

                //_signalRLocation.OnMessageReceivedLocation += _signalRLocation_OnMessageReceivedLocation;

                //string uri = "https://fixpro.engprosoft.net/XMLData/" + OneEmployee.Id + ".xml";

                //document = XDocument.Load(uri);

                //ds.Clear();
                //ds.ReadXml(new XmlTextReader(new StringReader(document.ToString())));

                //employeesTable = ds.Tables[0];

                //CurrentTrack = (from DataRow dr in employeesTable.Rows
                //                select new DataMapsModel()
                //                {
                //                    Id = int.Parse(dr["Tracking_id"].ToString()),
                //                    EmployeeId = int.Parse(dr["EmployeeId"].ToString()),
                //                    Lat = dr["lat"].ToString(),
                //                    Long = dr["log"].ToString(),
                //                    Time = dr["time"].ToString(),
                //                    CreateDate = dr["date"].ToString(),
                //                    MPosition = new Location(double.Parse(dr["lat"].ToString()), double.Parse(dr["log"].ToString())),
                //                }).FirstOrDefault();

                //LastListmap.Clear();
                //LastListmap.Add(CurrentTrack);
                //MapsModel = CurrentTrack;
            }
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task SelectedEmployeeInMap(TimeSheetResponse employee)
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                //await GetDataEmployee();
                //if (MapsModel != null)
                //{
                UserDialogs.Instance.ShowLoading();
                await App.Current!.MainPage!.Navigation.PushAsync(new Pages.TrackingPages.TrckingMapPage(new EmployeesViewModel(employee, ORep, _service), ORep, _service, _signalRService));
                UserDialogs.Instance.HideHud();
                //}
                //else
                //{
                //    var toast = Toast.Make(AppResources.msgNoavailablelocationcoordinates, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                //    await toast.Show();
                //}
            }

            IsEnable = true;
        }

        [RelayCommand]
        async Task GoHistoryTracking()
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                //if (StaticMember.CheckPermission(ApiConstants.GetHistoryLocationTimeSheet))
                //{
                    UserDialogs.Instance.ShowLoading();
                    await App.Current!.MainPage!.Navigation.PushAsync(new Pages.TrackingPages.EmployeesBranchHistoryPage(new HistoryTrackingViewModel(ORep, _service), ORep, _service));
                    UserDialogs.Instance.HideHud();
               //}
               // else
               // {
               //     var toast = Toast.Make($"{AppResources.mshPermissionToViewData}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
               //     await toast.Show();
              //  }
            }

            IsEnable = true;
        }
        #endregion
    }
}

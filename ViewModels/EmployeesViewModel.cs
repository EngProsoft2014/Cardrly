using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.Card;
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
        #endregion

        #region Prop
        [ObservableProperty]
        ObservableCollection<CardResponse> lstEmployeesInBranch;

        [ObservableProperty]
        ObservableCollection<CardResponse> lstEmployees;

        [ObservableProperty]
        ObservableCollection<CardResponse> lstEmployeesOnePage;

        [ObservableProperty]
        ObservableCollection<CardResponse> lstWorkingEmployees;

        [ObservableProperty]
        ObservableCollection<DataMapsModel> listmap;

        [ObservableProperty]
        ObservableCollection<DataMapsModel> lastListmap;

        [ObservableProperty]
        CardResponse oneEmployee;

        [ObservableProperty]
        CardResponse employeePermission;

        [ObservableProperty]
        bool isRefresh;

        public int PageNumber { get; set; }

        public int TotalPage { get; set; }

        public static DataMapsModel MapsModel { get; set; }

        SignalRService _signalRLocation;

        DataTable employeesTable;

        DataMapsModel CurrentTrack { get; set; }
        DataSet ds = new DataSet();
        XDocument document = new XDocument();
        #endregion

        #region Cons
        public EmployeesViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            ORep = GenericRep;
            _service = service;
            _signalRLocation = new SignalRService(_service);
            Init();
            Listmap = new ObservableCollection<DataMapsModel>();
            LastListmap = new ObservableCollection<DataMapsModel>();
        }

        //Tracking Constructor
        public EmployeesViewModel(CardResponse employee, IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            ORep = GenericRep;
            _service = service;
            _signalRLocation = new SignalRService(_service);
            OneEmployee = employee;
            Listmap = new ObservableCollection<DataMapsModel>();
            LastListmap = new ObservableCollection<DataMapsModel>();
            CurrentTrack = new DataMapsModel();
            //GetDataEmployee();
            //new Timer((Object stateInfo) => { GetDataEmployee(); }, new AutoResetEvent(false), 0, 3000);
        }

        //Employees Working Today Constructor
        public EmployeesViewModel(string startTracking, IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            ORep = GenericRep;
            _service = service;
            InitTraking();
        }
        #endregion

        #region Methods
        async void Init()
        {
            PageNumber = 1;
            LstEmployeesOnePage = new ObservableCollection<CardResponse>();
            LstEmployees = new ObservableCollection<CardResponse>();
            LstEmployeesInBranch = new ObservableCollection<CardResponse>();

            GetPerrmission();

            if (Controls.StaticMember.EmployeesPages == 2)
            {
                await GetEmployees();
            }

        }

        async void InitTraking()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                LstWorkingEmployees = new ObservableCollection<CardResponse>();

                string Date = DateTime.Now.ToString("yyyy-MM-dd");

                await Controls.StaticMember.GetWorkingEmployees(Preferences.Default.Get(ApiConstants.AccountId, ""), Date);

                LstWorkingEmployees = Controls.StaticMember.LstWorkingEmployeesStatic;
            }
        }

        //Get Perrmission for User
        public async void GetPerrmission()
        {
            //if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            //{
                //EmployeePermission = new CardResponse();
                //await Controls.StaticMember.CheckPermissionEmployee();
                //EmployeePermission = Controls.StaticMember.EmployeeDataStatic;
            //}
        }

        //Get All Employees
        public async Task GetEmployees()
        {
            //if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            //{
            //    UserDialogs.Instance.ShowLoading();

            //    string UserToken = await _service.UserToken();

            //    var json = await ORep.GetAsync<EmployeesInPageModel>(string.Format("api/Employee/GetEmpInPage/{0}/{1}/{2}/{3}/{4}", PageNumber, Helpers.Settings.AccountIdGet, Helpers.Settings.BranchIdGet, Controls.StartData.EmployeeDataStatic.UserRole, Helpers.Settings.UserIdGet), UserToken);

            //    if (json != null)
            //    {
            //        EmployeesInPageModel Employee = json;
            //        TotalPage = Employee.Pages;
            //        PageNumber += 1;

            //        LstEmployeesOnePage = new ObservableCollection<CardResponse>(Employee.EmployeesInPage);

            //        if (LstEmployees.Count == 0)
            //        {
            //            LstEmployees = new ObservableCollection<CardResponse>(LstEmployeesOnePage.OrderBy(x => x.UserName).ToList());
            //        }
            //        else
            //        {
            //            if (LstEmployees != LstEmployeesOnePage)
            //            {
            //                LstEmployees = new ObservableCollection<CardResponse>(LstEmployees.Concat(LstEmployeesOnePage).OrderBy(x => x.UserName).ToList());
            //            }
            //        }
            //    }

            //    UserDialogs.Instance.HideHud();
            //}

        }

        private async Task GetDataEmployee()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {

                _signalRLocation.OnMessageReceivedLocation += _signalRLocation_OnMessageReceivedLocation;

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

        private void _signalRLocation_OnMessageReceivedLocation(DataMapsModel locationData)
        {
            if (locationData.EmployeeId.ToString() == OneEmployee.Id)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Update the UI with the latest location
                    MapsModel = new DataMapsModel
                    {
                        Id = locationData.Id,
                        EmployeeId = locationData.EmployeeId,
                        Lat = locationData.Lat,
                        Long = locationData.Long,
                        Time = locationData.Time,
                        CreateDate = locationData.CreateDate,
                        MPosition = new Location(double.Parse(locationData.Lat), double.Parse(locationData.Long))
                    };
                });
            }
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async void SelectedEmployeeInMap(CardResponse employee)
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                await GetDataEmployee();
                if (MapsModel != null)
                {
                    UserDialogs.Instance.ShowLoading();
                    await App.Current!.MainPage!.Navigation.PushAsync(new Pages.TrackingPages.TrckingMapPage(MapsModel, new EmployeesViewModel(employee, ORep, _service), ORep, _service));
                    UserDialogs.Instance.HideHud();
                }
                else
                {
                    var toast = Toast.Make(AppResources.msgNoavailablelocationcoordinates, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }

            IsEnable = true;
        }
        #endregion
    }
}

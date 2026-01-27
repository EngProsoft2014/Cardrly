
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.TimeSheet;
using Cardrly.Models.TimeSheetBranch;
using Cardrly.Models.TimeSheetEmployeeBranch;
using Cardrly.Pages;
using Cardrly.Pages.MainPopups;
using Cardrly.Pages.TrackingPages;
using Cardrly.Services.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class HistoryTrackingViewModel : BaseViewModel
    {
        #region Service
        readonly IGenericRepository ORep;
        readonly ServicesService _service;
        #endregion

        [ObservableProperty]
        ObservableCollection<TimeSheetResponse> lstTimeSheet = [];

        [ObservableProperty]
        ObservableCollection<TimeSheetBranchResponse> lstBranches = [];
        [ObservableProperty]
        TimeSheetBranchResponse branchSelected = new TimeSheetBranchResponse();

        [ObservableProperty]
        ObservableCollection<TimeSheetEmployeeBranchResponse> lstEmployeesInBranch = [];
        [ObservableProperty]
        TimeSheetEmployeeBranchResponse employeeSelected = new TimeSheetEmployeeBranchResponse();

        [ObservableProperty]
        List<EmployeeLocationResponse> lstEmployeeLocations = [];

        [ObservableProperty]
        DateTime dateTracking = DateTime.UtcNow.Date;

        public HistoryTrackingViewModel(IGenericRepository GenericRep, ServicesService service)
        {
            ORep = GenericRep;
            _service = service;

            Init();
        }

        async void Init()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                string AccountId = Preferences.Default.Get(ApiConstants.AccountId, "");
                //string TimeSheetId = Preferences.Default.Get(ApiConstants.TimeSheetId, "");

                UserDialogs.Instance.ShowLoading();

                string UserToken = await _service.UserToken();
                //Get all Employees in this branch
                var json = await ORep.GetAsync<ObservableCollection<TimeSheetBranchResponse>>(ApiConstants.GetAllBranchesTimeSheetApi + AccountId, UserToken);

                if (json != null)
                {
                    LstBranches = json;
                }

                UserDialogs.Instance.HideHud();
            }
        }

        async Task GetData()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                if (!string.IsNullOrEmpty(BranchSelected?.Id) && DateTracking.Date <= DateTime.UtcNow.Date)
                {
                    string UserToken = await _service.UserToken();
                    //Get all TimeSheets for employee in this branch on selected date
                    UserDialogs.Instance.ShowLoading();

                    ObservableCollection<TimeSheetResponse> json;
                    if (!string.IsNullOrEmpty(EmployeeSelected.Id))
                        json = await ORep.GetAsync<ObservableCollection<TimeSheetResponse>>($"{ApiConstants.GetTimeSheetsByEmployeeTimeSheetApi}{BranchSelected.Id}/{DateTracking.Date.ToString("yyyy-MM-dd")}?CardId={EmployeeSelected.CardId}", UserToken);
                    else
                        json = await ORep.GetAsync<ObservableCollection<TimeSheetResponse>>($"{ApiConstants.GetTimeSheetsByEmployeeTimeSheetApi}{BranchSelected.Id}/{DateTracking.Date.ToString("yyyy-MM-dd")}", UserToken);

                    UserDialogs.Instance.HideHud();

                    if (json != null)
                    {
                        LstTimeSheet = new ObservableCollection<TimeSheetResponse>(json);
                    }
                }
                else
                {
                    LstTimeSheet = new ObservableCollection<TimeSheetResponse>();
                }
            }
        }

        [RelayCommand]
        async Task SelectBranch(TimeSheetBranchResponse branch)
        {
            BranchSelected = branch;
            if(branch?.TimeSheetEmployeeBranches != null && branch?.TimeSheetEmployeeBranches.Count > 0)
            {
                LstEmployeesInBranch = new ObservableCollection<TimeSheetEmployeeBranchResponse>(branch.TimeSheetEmployeeBranches);
            }

            await GetData();
        }

        [RelayCommand]
        async Task SelectEmployee(TimeSheetEmployeeBranchResponse employee)
        {
            EmployeeSelected =  employee;
            await GetData();
        }

        [RelayCommand]
        async Task SelectDatePicker()
        {
            await GetData();
        }


        [RelayCommand]
        async Task SelectedTimeSheet(TimeSheetResponse timeSheet)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                string UserToken = await _service.UserToken();
                //Get all Locations for employee in this branch on selected date
                UserDialogs.Instance.ShowLoading();
                var json = await ORep.GetAsync<List<EmployeeLocationResponse>>($"{ApiConstants.GetEmployeeLocationsTimeSheetApi}{timeSheet.Id}", UserToken);
                UserDialogs.Instance.HideHud();

                if (json != null)
                {
                    LstEmployeeLocations = json;

                    await App.Current!.MainPage!.Navigation.PushAsync(new HistoryTrackingMapPage(LstEmployeeLocations, timeSheet.CardName));
                }
            }
        }
    }
}

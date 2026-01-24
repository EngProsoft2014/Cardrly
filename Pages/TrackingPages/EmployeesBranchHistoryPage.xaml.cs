
using Cardrly.Helpers;
using Cardrly.Models.TimeSheetBranch;
using Cardrly.Models.TimeSheetEmployeeBranch;
using Cardrly.ViewModels;
using System.Collections.ObjectModel;

namespace Cardrly.Pages.TrackingPages;

public partial class EmployeesBranchHistoryPage : Controls.CustomControl
{
    #region Service
    readonly IGenericRepository ORep;
    readonly Services.Data.ServicesService _service;
    #endregion
    HistoryTrackingViewModel historyTrackingViewModel;

    public EmployeesBranchHistoryPage(HistoryTrackingViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        ORep = GenericRep;
        _service = service;
        this.BindingContext = historyTrackingViewModel = model;
    }

    private void srcBarEmployee_TextChanged(object sender, TextChangedEventArgs e)
    {
        lstTimeSheets.ItemsSource = historyTrackingViewModel.LstTimeSheet.Where(x => (x.CardName!).Contains(srcBarEmployee.Text.ToLower()));
    }

    private void BranchesPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var selectedBranch = picker?.SelectedItem as TimeSheetBranchResponse;

        historyTrackingViewModel.SelectBranchCommand.Execute(selectedBranch!);
    }

    private void EmployeesPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var selectedEmployee = picker?.SelectedItem as TimeSheetEmployeeBranchResponse;

        historyTrackingViewModel.SelectEmployeeCommand.Execute(selectedEmployee!);
    }

    private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        historyTrackingViewModel.SelectDatePickerCommand.Execute(null);
    }
}
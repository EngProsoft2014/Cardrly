using Cardrly.Helpers;
using Cardrly.ViewModels;

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
}
using Cardrly.Helpers;
using Cardrly.ViewModels;

namespace Cardrly.Pages.TrackingPages;

public partial class EmployeesWorkingPage : Controls.CustomControl
{
    #region Service
    readonly IGenericRepository ORep;
    readonly Services.Data.ServicesService _service;
    #endregion
    EmployeesViewModel employeesViewModel;

    public EmployeesWorkingPage(EmployeesViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        ORep = GenericRep;
        _service = service;
        this.BindingContext = employeesViewModel = model;
    }

    private void srcBarEmployee_TextChanged(object sender, TextChangedEventArgs e)
    {
        lstEmployees.ItemsSource = employeesViewModel.LstWorkingEmployees.Where(x => (x.CardName!).Contains(srcBarEmployee.Text.ToLower()));
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        MessagingCenter.Send(this, "ChangeEmployeeTime", true);
        await Navigation.PopAsync();
    }
}
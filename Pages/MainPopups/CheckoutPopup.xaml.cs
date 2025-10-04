using CommunityToolkit.Maui.Alerts;
using Cardrly.Helpers;
using Cardrly.ViewModels;
using Mopups.Services;

namespace Cardrly.Pages.MainPopups;

public partial class CheckoutPopup : Mopups.Pages.PopupPage
{
    public delegate void TimeDelegte(TimeSpan time);
    public event TimeDelegte TimeDidClose;

    #region Service
    readonly IGenericRepository ORep;
    readonly Services.Data.ServicesService _service;
    #endregion
    TimeSheetViewModel timeSheetViewModel;

    public CheckoutPopup(TimeSheetViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        ORep = GenericRep;
        _service = service;
        this.BindingContext = timeSheetViewModel = model;
    }

    public CheckoutPopup(int id, string time, TimeSheetViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        ORep = GenericRep;
        _service = service;
        this.BindingContext = timeSheetViewModel = model;

        btnCheck.Text = "Check In";

        if (time != null && time != "")
        {
            timeCheckOut.Time = TimeSpan.Parse(time);
        }
    }

    public CheckoutPopup(string time, TimeSheetViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        ORep = GenericRep;
        _service = service;
        this.BindingContext = timeSheetViewModel = model;

        btnCheck.Text = "Check Out";

        if (time != null && time != "")
        {
            timeCheckOut.Time = TimeSpan.Parse(time);
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (timeCheckOut.Time != null)
        {
            TimeDidClose?.Invoke(timeCheckOut.Time);
        }
        else
        {
            var toast = Toast.Make("Please select a time.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }

        MessagingCenter.Send(this, "ChangeEmployeeTime", true);
        await MopupService.Instance.PopAsync();
    }
}
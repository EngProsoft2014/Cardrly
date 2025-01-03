using Cardrly.Models;
using Mopups.Services;
using Syncfusion.Maui.Calendar;

namespace Cardrly.Pages.MainPopups;

public partial class DatePopup : Mopups.Pages.PopupPage
{
    public delegate void RangeDelegte(CalendarModel calendar);
    public event RangeDelegte RangeClose;

    CalendarModel Cal;
    public DatePopup()
    {
        InitializeComponent();

    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void btnOk_Clicked(object sender, EventArgs e)
    {
        Cal = new CalendarModel();

        if (calendar.SelectedDateRange != null)
        {
            Cal.SelectedRange = (CalendarDateRange)calendar.SelectedDateRange;

            Cal.StartDate = Cal.SelectedRange.StartDate;
            Cal.EndDate = Cal.SelectedRange.EndDate;

            RangeClose?.Invoke(Cal);

        }
        else if (calendar.SelectedDate != null)
        {
            Cal.StartDate = Cal.EndDate = calendar.SelectedDate.Value;

            RangeClose?.Invoke(Cal);
        }
        else
        {
            await App.Current.MainPage.DisplayAlert("Alert", "Please select a date", "Ok");
        }

        await MopupService.Instance.PopAsync();

    }
}
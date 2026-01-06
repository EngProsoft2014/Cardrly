using CommunityToolkit.Maui.Alerts;
using Cardrly.Models;
using Mopups.Services;
using Syncfusion.Maui.Calendar;

namespace Cardrly.Pages.MainPopups;


public partial class DatePopup : Mopups.Pages.PopupPage
{
    //public delegate void RangeDelegte(CalendarModel calendar);
    //public event RangeDelegte RangeClose;
    private TaskCompletionSource<CalendarModel?> _tcs;
    CalendarModel Cal;
    public DatePopup(TaskCompletionSource<CalendarModel?> tsk)
    {
        InitializeComponent();
        _tcs = tsk;
        if (Controls.StaticMember.SelectedDate != null)
        {
            calendar.SelectedDate = Controls.StaticMember.SelectedDate;
        }

    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _tcs.TrySetResult(null);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        //RangeClose = null;
        _tcs.TrySetResult(null);
        await MopupService.Instance.PopAsync();
    }

    private async void btnOk_Clicked(object sender, EventArgs e)
    {
        Cal = new CalendarModel();

        if (calendar.SelectedDateRange != null)
        {
            Cal.SelectedRange = (CalendarDateRange)calendar.SelectedDateRange;

            Controls.StaticMember.SelectedDate = Cal.SelectedRange.StartDate.Value;

            Cal.StartDate = Cal.SelectedRange.StartDate;
            Cal.EndDate = Cal.SelectedRange.EndDate;

            //RangeClose?.Invoke(Cal);

        }
        else if (calendar.SelectedDate != null)
        {
            Cal.StartDate = Cal.EndDate = calendar.SelectedDate.Value;

            Controls.StaticMember.SelectedDate = calendar.SelectedDate.Value;

            //RangeClose?.Invoke(Cal);
        }
        else
        {
            var toast = Toast.Make("Please select a date.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
        //RangeClose = null;
        _tcs.TrySetResult(Cal);
        await MopupService.Instance.PopAsync();

    }
}
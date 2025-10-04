using Akavache;
using Cardrly.ViewModels;

namespace Cardrly.Pages;

public partial class TimeSheetPage : Controls.CustomControl
{
    TimeSheetViewModel timeSheetViewModel;

    public TimeSheetPage(TimeSheetViewModel viewModel)
	{
		InitializeComponent();

		this.BindingContext = timeSheetViewModel = viewModel;
	}


    private void CheckInTapped(object sender, TappedEventArgs e)
    {
        stkClockIN.IsVisible = true;
        stkClockOUT.IsVisible = false;
        lstEmployeesOut.IsVisible = false;
        lstEmployeesIn.IsVisible = true;

        if (timeSheetViewModel.LstEmployeesIn.Count == 0)
        {
            stkNoDataIN.IsVisible = true;
            stkNoDataOUT.IsVisible = false;
        }
        else
        {
            stkNoDataIN.IsVisible = false;
            stkNoDataOUT.IsVisible = true;
        }
    }

    private void ChecOutTapped(object sender, TappedEventArgs e)
    {
        stkClockIN.IsVisible = false;
        stkClockOUT.IsVisible = true;
        lstEmployeesIn.IsVisible = false;
        lstEmployeesOut.IsVisible = true;

        if (timeSheetViewModel.LstEmployeesOut.Count == 0)
        {
            stkNoDataOUT.IsVisible = true;
            stkNoDataIN.IsVisible = false;
        }
        else
        {
            stkNoDataOUT.IsVisible = false;
            stkNoDataIN.IsVisible = true;
        }
    }


    private void TimeSheet_SelectionChanged(object sender, Syncfusion.Maui.TabView.TabSelectionChangedEventArgs e)
    {
        if (e.NewIndex == 0)
        {
            stkClockIN.IsVisible = true;
            stkClockOUT.IsVisible = false;
            lstEmployeesOut.IsVisible = false;
            lstEmployeesIn.IsVisible = true;

            if (timeSheetViewModel.LstEmployeesIn.Count == 0)
            {
                stkNoDataIN.IsVisible = true;
                stkNoDataOUT.IsVisible = false;
            }
            else
            {
                stkNoDataIN.IsVisible = false;
                stkNoDataOUT.IsVisible = true;
            }
        }
        else if (e.NewIndex == 1)
        {
            stkClockIN.IsVisible = false;
            stkClockOUT.IsVisible = true;
            lstEmployeesIn.IsVisible = false;
            lstEmployeesOut.IsVisible = true;

            if (timeSheetViewModel.LstEmployeesOut.Count == 0)
            {
                stkNoDataOUT.IsVisible = true;
                stkNoDataIN.IsVisible = false;
            }
            else
            {
                stkNoDataOUT.IsVisible = false;
                stkNoDataIN.IsVisible = true;
            }
        }
    }
}
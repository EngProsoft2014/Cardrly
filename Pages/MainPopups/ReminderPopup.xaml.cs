using Cardrly.Constants;
using Cardrly.Models;
using Cardrly.Resources.Lan;
using Mopups.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Cardrly.Pages.MainPopups;

public partial class ReminderPopup : Mopups.Pages.PopupPage
{
    public delegate void ReminderDelegte(DateTime date);
    public event ReminderDelegte ReminderClose;
    ObservableCollection<ReminderModel> ReminderLst = new ObservableCollection<ReminderModel>();
	public ReminderPopup()
	{
		InitializeComponent();
		Init();

        string Lan = Preferences.Default.Get("Lan", "en");

        if (Lan == "ar")
        {
            this.FlowDirection = FlowDirection.RightToLeft;
            CultureInfo.CurrentCulture = new CultureInfo("ar");
        }
        else
        {
            this.FlowDirection = FlowDirection.LeftToRight;
            CultureInfo.CurrentCulture = new CultureInfo("en");
        }
    }


	void Init()
	{
		ReminderLst.Add(new ReminderModel { Name = $"{AppResources.lblNow}",Date = DateTime.Now});
		ReminderLst.Add(new ReminderModel { Name = $"{AppResources.lbl1hour}",Date = DateTime.Now.AddHours(1)});
		ReminderLst.Add(new ReminderModel { Name = $"{AppResources.lbl1day}",Date = DateTime.Now.AddDays(1)});
		ReminderLst.Add(new ReminderModel { Name = $"{AppResources.lbl3days}",Date = DateTime.Now.AddDays(3)});
		ReminderLst.Add(new ReminderModel { Name = $"{AppResources.lbl1week}",Date = DateTime.Now.AddDays(7)});
		ReminderLst.Add(new ReminderModel { Name = $"{AppResources.lbl1month}",Date = DateTime.Now.AddMonths(1)});

		ReminderColc.ItemsSource = ReminderLst;
	}

    private async void Colc_Tapped(object sender, TappedEventArgs e)
    {
		var item = (ReminderModel)e.Parameter;
        //Update the date based on the selected item
        if (item.Name == AppResources.lblNow)
		{
			item.Date = DateTime.Now;
        }
		else if (item.Name == AppResources.lbl1hour)
		{
            item.Date = DateTime.Now.AddHours(1);
        }
        else if (item.Name == AppResources.lbl1day)
        {
            item.Date = DateTime.Now.AddDays(1);
        }
        else if (item.Name == AppResources.lbl3days)
        {
            item.Date = DateTime.Now.AddDays(3);
        }
        else if (item.Name == AppResources.lbl1week)
        {
            item.Date = DateTime.Now.AddDays(7);
        }
        else if (item.Name == AppResources.lbl1month)
        {
            item.Date = DateTime.Now.AddMonths(1);
        }
        await MopupService.Instance.PopAsync();
		ReminderClose.Invoke(item!.Date);
    }

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }
}
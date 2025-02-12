using Cardrly.Constants;
using Cardrly.Models;
using Cardrly.Resources.Lan;
using Mopups.Services;
using System.Collections.ObjectModel;

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
        await MopupService.Instance.PopAsync();
		ReminderClose.Invoke(item!.Date);
    }
}
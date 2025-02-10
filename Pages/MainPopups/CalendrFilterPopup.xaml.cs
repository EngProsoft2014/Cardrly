using Cardrly.Mode_s.Card;
using Cardrly.Models.Calendar;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using Mopups.Services;
using System.Collections.ObjectModel;

namespace Cardrly.Pages.MainPopups;

public partial class CalendrFilterPopup : Mopups.Pages.PopupPage
{
    public delegate void FilterDelegte(string From, string To, CalendarTypeItemModel calendarType, CardResponse card);
    public event FilterDelegte FilterClose;
    public CalendrFilterPopup(ObservableCollection<CalendarTypeItemModel> calendarTypes,ObservableCollection<CardResponse> cardLst)
	{
		InitializeComponent();
        this.BindingContext = this;
        ProviderPicker.ItemsSource = calendarTypes;
        ProviderPicker.SelectedItem = calendarTypes[0];
        CalenderCardPicker.ItemsSource = cardLst;
        if (cardLst.Count > 0)
        {
            CalenderCardPicker.SelectedItem = cardLst[0];
        }
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        if (FromPicker.Date > ToPicker.Date)
        {
            var toast = Toast.Make($"{AppResources.msgDate_HomePage}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
        else
        {
            await MopupService.Instance.PopAsync();
            FilterClose?.Invoke(FromPicker.Date.ToString("yyyy-MM-dd"), ToPicker.Date.ToString("yyyy-MM-dd"), (CalendarTypeItemModel)ProviderPicker.SelectedItem, (CardResponse)CalenderCardPicker.SelectedItem);
        }
        
    }
}
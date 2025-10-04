using Cardrly.Models.Card;
using Cardrly.Models.Calendar;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using Mopups.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace Cardrly.Pages.MainPopups;

public partial class CalendrFilterPopup : Mopups.Pages.PopupPage
{
    public delegate void FilterDelegte(string From, string To, CalendarTypeItemModel calendarType, CardResponse card);
    public event FilterDelegte FilterClose;
    public CalendrFilterPopup(ObservableCollection<CalendarTypeItemModel> calendarTypes,ObservableCollection<CardResponse> cardLst)
	{
		InitializeComponent();
        //this.BindingContext = this;
        ProviderPicker.ItemsSource = calendarTypes;
        //ProviderPicker.SelectedItem = calendarTypes[0];
        CalenderCardPicker.ItemsSource = cardLst;
        //if (cardLst.Count > 0)
        //{
        //    CalenderCardPicker.SelectedItem = cardLst[0];
        //}

        string Lan = Preferences.Default.Get("Lan", "en");
        if (Lan == "ar")
        {
            this.FlowDirection = FlowDirection.RightToLeft;
            FromPicker.FlowDirection = FlowDirection.RightToLeft;
            ToPicker.FlowDirection = FlowDirection.RightToLeft;
        }
        else
        {
            this.FlowDirection = FlowDirection.LeftToRight;
            FromPicker.FlowDirection = FlowDirection.LeftToRight;
            ToPicker.FlowDirection = FlowDirection.LeftToRight;
        }
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        this.IsEnabled = false;
        if (FromPicker.Date > ToPicker.Date)
        {
            var toast = Toast.Make($"{AppResources.msgDate_HomePage}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
        else 
        {
            if (ProviderPicker.SelectedItem == null || CalenderCardPicker.SelectedItem == null)
            {
                var toast = Toast.Make($"{AppResources.msgAll_Fields_is_Required}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                await MopupService.Instance.PopAsync();
                FilterClose?.Invoke(FromPicker.Date.ToString("yyyy-MM-dd"), ToPicker.Date.ToString("yyyy-MM-dd"), (CalendarTypeItemModel)ProviderPicker.SelectedItem, (CardResponse)CalenderCardPicker.SelectedItem);
            }
        }
        this.IsEnabled = true;
    }

    private void ProviderPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        PlaceholderProviderLabel.IsVisible = ProviderPicker.SelectedIndex == -1;
    }

    private void CalenderCardPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        PlaceholderCardLabel.IsVisible = CalenderCardPicker.SelectedIndex == -1;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }
}
using Cardrly.Enums;
using Cardrly.Models.Lead;
using Mopups.Services;
using System.Globalization;

namespace Cardrly.Pages.MainPopups;

public partial class LeadFilterPopup : Mopups.Pages.PopupPage
{
    public LeadFilterRequest FilterRequest = new LeadFilterRequest();
    public List<int> ShowingLst = new List<int>();
    public List<EnumSearchLead> SortByLst = new List<EnumSearchLead>();
    public List<string> AlphabetSortingLst = new List<string>();
    public delegate void FilterDelegte(LeadFilterRequest filterRequest);
    public event FilterDelegte FilterClose;
    public LeadFilterPopup(LeadFilterRequest model)
	{
		InitializeComponent();
        this.BindingContext = this;
        Init(model);

        // Flow Direction 
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

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }

    void Init(LeadFilterRequest model)
    {
        // Add To ShowingLst 
        ShowingLst.Add(25);
        ShowingLst.Add(50);
        ShowingLst.Add(100);
        ShowingLst.Add(200);
        ShowingLst.Add(500);
        ShowingLst.Add(1000);
        // Add To AlphabetSortingLst 
        AlphabetSortingLst.Add("A-Z");
        AlphabetSortingLst.Add("Z-A");
        // Add To SortByLst
        SortByLst = new List<EnumSearchLead>
        {
             EnumSearchLead.Name,
             EnumSearchLead.Date,          
             EnumSearchLead.Category,
             EnumSearchLead.Company
        };

        //binding pickers 
        ShowingPicker.ItemsSource = ShowingLst;
        AlphabetSortingPicker.ItemsSource = AlphabetSortingLst;
        SortbyPicker.ItemsSource = SortByLst;
        //Defult Selection
        ShowingPicker.SelectedItem = ShowingLst.FirstOrDefault(a =>a == model.Pagesize);
        AlphabetSortingPicker.SelectedItem = AlphabetSortingLst.FirstOrDefault(a => a == model.sortbydir);
        SortbyPicker.SelectedItem = SortByLst.FirstOrDefault(a=>(int)a == model.sortby);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        this.IsEnabled = false;
        EnumSearchLead SelectedSort = (EnumSearchLead)SortbyPicker.SelectedItem;
        FilterRequest = new LeadFilterRequest { Pagesize = (int)ShowingPicker.SelectedItem,
            sortbydir = (string)AlphabetSortingPicker.SelectedItem,
            sortby = ((int)(EnumSearchLead)SortbyPicker.SelectedItem)
        };
        FilterClose?.Invoke(FilterRequest);
        this.IsEnabled = true;
    }
}
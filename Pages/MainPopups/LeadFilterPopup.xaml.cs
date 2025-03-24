using Cardrly.Enums;
using Cardrly.Models.Lead;
using Mopups.Services;

namespace Cardrly.Pages.MainPopups;

public partial class LeadFilterPopup : Mopups.Pages.PopupPage
{
    public LeadFilterRequest FilterRequest = new LeadFilterRequest();
    public List<int> ShowingLst = new List<int>();
    public List<EnumSearchLead> SortByLst = new List<EnumSearchLead>();
    public List<string> AlphabetSortingLst = new List<string>();
    public delegate void FilterDelegte(LeadFilterRequest filterRequest);
    public event FilterDelegte FilterClose;
    public LeadFilterPopup()
	{
		InitializeComponent();
        this.BindingContext = this;
        Init();
	}

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }

    void Init()
    {
        // Add To ShowingLst 
        ShowingLst.Add(25);
        ShowingLst.Add(50);
        ShowingLst.Add(75);
        ShowingLst.Add(100);
        ShowingLst.Add(150);
        // Add To AlphabetSortingLst 
        AlphabetSortingLst.Add("A-Z");
        AlphabetSortingLst.Add("Z-A");
        // Add To SortByLst
        SortByLst = new List<EnumSearchLead>
        {
            EnumSearchLead.Date,
            EnumSearchLead.Name,
            EnumSearchLead.Category,
            EnumSearchLead.Company
        };

        //binding pickers 
        ShowingPicker.ItemsSource = ShowingLst;
        AlphabetSortingPicker.ItemsSource = AlphabetSortingLst;
        SortbyPicker.ItemsSource = SortByLst;
        //Defult Selection
        ShowingPicker.SelectedItem = ShowingLst[0];
        AlphabetSortingPicker.SelectedItem = AlphabetSortingLst[0];
        SortbyPicker.SelectedItem = SortByLst[0];
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
using Cardrly.Constants;
using Cardrly.Enums;
using Cardrly.Helpers;
using Cardrly.Models.Card;
using Cardrly.Models.Lead;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using Mopups.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace Cardrly.Pages.MainPopups;

public partial class LeadFilterPopup : Mopups.Pages.PopupPage
{
    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion

    public LeadFilterRequest FilterRequest = new LeadFilterRequest();
    public List<int> ShowingLst = new List<int>();
    public List<EnumSearchLead> SortByLst = new List<EnumSearchLead>();
    public List<string> AlphabetSortingLst = new List<string>();
    public List<string> CreadtedByLst = new List<string>();
    public ObservableCollection<CardResponse> CardLst = new ObservableCollection<CardResponse>();

    public delegate void FilterDelegte(LeadFilterRequest filterRequest);
    public event FilterDelegte FilterClose;
    public LeadFilterPopup(LeadFilterRequest model, IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        Rep = GenericRep;
        _service = service;
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

    async Task Init(LeadFilterRequest model)
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

        CreadtedByLst.Add("None");
        CreadtedByLst.Add("Created-date");
        CreadtedByLst.Add("Created-Comment"); 

        DateFromPicker.Date = DateTime.UtcNow.AddDays(-10);
        DateToPicker.Date = DateTime.UtcNow;

        await GetAllCards();

        //binding pickers 
        ShowingPicker.ItemsSource = ShowingLst;
        AlphabetSortingPicker.ItemsSource = AlphabetSortingLst;
        SortbyPicker.ItemsSource = SortByLst;
        CreatedByPicker.ItemsSource = CreadtedByLst;
        CardPicker.ItemsSource = CardLst;
        //Defult Selection
        ShowingPicker.SelectedItem = ShowingLst.FirstOrDefault(a =>a == model.Pagesize);
        AlphabetSortingPicker.SelectedItem = AlphabetSortingLst.FirstOrDefault(a => a == model.sortbydir);
        SortbyPicker.SelectedItem = SortByLst.FirstOrDefault(a=>(int)a == model.sortby);
        CreatedByPicker.SelectedItem = CreadtedByLst.FirstOrDefault(a=>a == model.rangetype);
        CardPicker.SelectedItem = CardLst.FirstOrDefault(a => a.Id == model.CardId);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        this.IsEnabled = false;

        try
        {
            // Cache common values so we don’t repeat casts
            var selectedRange = CreatedByPicker.SelectedItem?.ToString() ?? string.Empty;

            if (selectedRange == "Created-date" && (DateFromPicker.Date > DateToPicker.Date))
            {
                var toast = Toast.Make($"{AppResources.msgDate_HomePage}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                // Cache common values so we don’t repeat casts        
                var pageSize = (int)ShowingPicker.SelectedItem;
                var sortDir = (string)AlphabetSortingPicker.SelectedItem;
                var sortBy = (int)(EnumSearchLead)SortbyPicker.SelectedItem;

                FilterRequest = selectedRange switch
                {
                    "None" => new LeadFilterRequest
                    {
                        Pagesize = pageSize,
                        sortbydir = sortDir,
                        sortby = sortBy,
                        rangetype = selectedRange,
                    },

                    _ => new LeadFilterRequest
                    {
                        Pagesize = pageSize,
                        sortbydir = sortDir,
                        sortby = sortBy,
                        rangetype = selectedRange,
                        fromdt = DateOnly.FromDateTime(DateFromPicker.Date),
                        todt = DateOnly.FromDateTime(DateToPicker.Date)
                    }
                };

                var card = CardPicker.SelectedItem as CardResponse;
                FilterRequest.CardId = card!.Id;

                FilterClose?.Invoke(FilterRequest);
            }

        }
        finally
        {
            this.IsEnabled = true;
        }      
    }

    private void CreatedByPicker_SelectedIndexChanged(object sender, EventArgs e)
    {     
        if (CreatedByPicker.SelectedItem.ToString() != "None")
        {
            stkDate.IsVisible = true;
        }
        else
        {
            stkDate.IsVisible = false;
        }
    }

    public async Task GetAllCards()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

                var json = await Rep.GetAsync<ObservableCollection<CardResponse>>($"{ApiConstants.CardGetAllApi}{AccId}/Card", UserToken);

                if (json != null && json.Count > 0)
                {
                    CardLst = json;
                    CardLst.Insert(0, new CardResponse { Id = "", CardName = "None" });
                }
            }
        }
    }
}
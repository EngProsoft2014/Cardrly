using Cardrly.Helpers;
using Cardrly.ViewModels;
using Cardrly.ViewModels.Leads;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using static Cardrly.Models.Calendar.CalendlyResponseModel;
using static Cardrly.Models.Calendar.GmailResponseModel;

namespace Cardrly.Pages;

public partial class HomePage : Controls.CustomControl
{
    #region Prop
    CardsViewModel cardsViewModel;
    HomeViewModel homeViewModel;
    LeadViewModel LeadViewModel;
    CalendarViewModel CalendarViewModel;
    #endregion

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion

    public HomePage(HomeViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        homeViewModel = model;
        //CardPicker.SelectedIndex = 0;
        Rep = GenericRep;
        _service = service;
        HomeView.BindingContext = model;

        // Add Flow Direction For Content View 
        HomeView.FlowDirection = this.FlowDirection;
        CardsView.FlowDirection = this.FlowDirection;
        LeadView.FlowDirection = this.FlowDirection;
        CalendarView.FlowDirection = this.FlowDirection;
        MoreView.FlowDirection = this.FlowDirection;
    }

    #region Methods
    private void SfTabView_SelectionChanged(object sender, Syncfusion.Maui.TabView.TabSelectionChangedEventArgs e)
    {

        if (e.NewIndex == 0)
        {
            HomeView.BindingContext = homeViewModel;
        }
        else if (e.NewIndex == 1)
        {
            if (homeViewModel.IsEnable == true)
            {
                CardsView.BindingContext = cardsViewModel = new CardsViewModel(homeViewModel.CardLst, Rep, _service);
            }
            else
            {
                tabHome.SelectedIndex = 0;
            }
        }
        else if (e.NewIndex == 2)
        {
            if (homeViewModel.IsEnable == true)
            {
                LeadView.BindingContext = LeadViewModel = new LeadViewModel(Rep, _service, homeViewModel._audioManager);
            }
            else
            {
                tabHome.SelectedIndex = 0;
            }
        }
        else if (e.NewIndex == 3)
        {
            if (homeViewModel.IsEnable == true)
            {
                CalendarView.BindingContext = CalendarViewModel = new CalendarViewModel(Rep, _service);
            }
            else
            {
                tabHome.SelectedIndex = 0;
            }
        }
        else if (e.NewIndex == 4)
        {
            if (homeViewModel.IsEnable == true)
            {
                MoreView.BindingContext = new MoreViewModel(Rep, _service, homeViewModel._audioManager);
            }
            else
            {
                tabHome.SelectedIndex = 0;
            }
        }

    }

    protected override bool OnBackButtonPressed()
    {
        // Run the async code on the UI thread
        Dispatcher.Dispatch(() =>
        {
            Action action = () => Application.Current!.Quit();
            Controls.StaticMember.ShowSnackBar("Do you want to exit the program", Controls.StaticMember.SnackBarColor, Controls.StaticMember.SnackBarTextColor, action);
        });

        // Return true to prevent the default behavior
        return true;
    }

    private void Cards_Refreshing(object sender, EventArgs e)
    {
        RefView.IsRefreshing = true;
        cardsViewModel.Init();
        RefView.IsRefreshing = false;
    }

    private void Lead_Refreshing(object sender, EventArgs e)
    {
        LeadRef.IsRefreshing = true;
        LeadViewModel.Init();
        LeadRef.IsRefreshing = false;
    }

    private async void SearchBar_Lead(object sender, TextChangedEventArgs e)
    {
        LeadViewModel.FilterRequest.SearchLead = e.NewTextValue;
        LeadViewModel.FilterRequest.PageNumber = 1;
        await LeadViewModel.SearchLeads();
    }

    private async void HomeRef_Refreshing(object sender, EventArgs e)
    {
        HomeRef.IsRefreshing = true;
        await homeViewModel.GetAccData();
        await homeViewModel.GetAllCards();
        HomeRef.IsRefreshing = false;
    }

    private void CalFromData_DateSelected(object sender, DateChangedEventArgs e)
    {
        CalendarViewModel.CalendlyResponses = new System.Collections.ObjectModel.ObservableCollection<CalendarCalendlyResponse>();
        CalendarViewModel.CalendarEventGmails = new System.Collections.ObjectModel.ObservableCollection<CalendarEventGmail>();
        CalendarViewModel.CalendarOutlookEvents = new System.Collections.ObjectModel.ObservableCollection<Models.Calendar.OutLookResponseModel.CalendarOutlookEvent>();
        CalendarViewModel.IsPassed = 2;
    }

    private void CalenderCardPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        CalendarViewModel.CalendlyResponses = new System.Collections.ObjectModel.ObservableCollection<CalendarCalendlyResponse>();
        CalendarViewModel.CalendarEventGmails = new System.Collections.ObjectModel.ObservableCollection<CalendarEventGmail>();
        CalendarViewModel.CalendarOutlookEvents = new System.Collections.ObjectModel.ObservableCollection<Models.Calendar.OutLookResponseModel.CalendarOutlookEvent>();
        CalendarViewModel.IsPassed = 2;
    }
    #endregion
}
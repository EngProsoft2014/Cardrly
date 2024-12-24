using Cardrly.Helpers;
using Cardrly.ViewModels;
using Cardrly.ViewModels.Leads;

namespace Cardrly.Pages;

public partial class HomePage : Controls.CustomControl
{
    CardsViewModel cardsViewModel;
    HomeViewModel homeViewModel;
    LeadViewModel LeadViewModel;
    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public HomePage(HomeViewModel model,IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        homeViewModel = model;
        CardPicker.SelectedIndex = 0;
        Rep = GenericRep;
        _service = service;
        HomeView.BindingContext = model;
    }

    private void SfTabView_SelectionChanged(object sender, Syncfusion.Maui.TabView.TabSelectionChangedEventArgs e)
    {
        if (e.NewIndex == 0)
        {
            HomeView.BindingContext = homeViewModel;
        }
		else if (e.NewIndex == 1)
		{
            CardsView.BindingContext = cardsViewModel = new CardsViewModel(homeViewModel.CardLst,Rep,_service);
		}
		else if (e.NewIndex == 2)
		{
			ContactView.BindingContext = LeadViewModel = new LeadViewModel(Rep,_service);
		}
		else if (e.NewIndex == 3)
		{
			MoreView.BindingContext = new MoreViewModel(Rep,_service);
		} 
    }

    [Obsolete]
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
        LeadRef.IsRefreshing= true;
        LeadViewModel.Init();
        LeadRef.IsRefreshing = false;
    }

    private void SearchBar_Lead(object sender, TextChangedEventArgs e)
    {
        LeadColc.ItemsSource = LeadViewModel.Leads.Where(a => a.FullName.Contains(e.NewTextValue));
    }
}

using Cardrly.Models.CardLink;
using Cardrly.ViewModels.Links;

namespace Cardrly.Pages.Links;

public partial class LinksPage : Controls.CustomControl
{
    LinksViewModel Model;
    public LinksPage(LinksViewModel model)
	{
		InitializeComponent();
        Model = model;
        
	}

    private void RefreshView_Refreshing(object sender, EventArgs e)
    {
		RefView.IsRefreshing = true;
        Model.Init(Model.CardId);
        RefView.IsRefreshing = false;
    }

    private async void Calc_ReorderCompleted(object sender, EventArgs e)
    {
        List<CardLinkSortRequest> sortRequests = new List<CardLinkSortRequest>();
        List<CardLinkResponse> items = (List<CardLinkResponse>) Calc.ItemsSource;
        foreach (CardLinkResponse item in Calc.ItemsSource)
        {
            var oldIndex = Model.CardOrder.IndexOf((CardLinkResponse)item);
            var newIndex = items.IndexOf((CardLinkResponse)item);
            if (oldIndex != newIndex)
            {
                sortRequests.Add(new CardLinkSortRequest { Id = item.Id , SortNumber = newIndex });
            }
        }
        Model.OrderList(Model.CardDetails.Id, sortRequests);
    }
}
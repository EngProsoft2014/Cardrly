using Cardrly.Mode_s.CardLink;
using Cardrly.ViewModels;
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
}
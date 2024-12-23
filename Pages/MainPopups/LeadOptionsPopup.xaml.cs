
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Controls.UserDialogs.Maui;

using Mopups.Services;
using System.Collections.ObjectModel;

namespace Cardrly.Pages.MainPopups;

public partial class LeadOptionsPopup : Mopups.Pages.PopupPage
{
    LeadResponse Res = new LeadResponse();

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public LeadOptionsPopup(LeadResponse res, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        Res = res;
    }

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void TapGestureRecognizer_Comment(object sender, TappedEventArgs e)
    {
        await MopupService.Instance.PopAsync();
        await MopupService.Instance.PushAsync(new CommentPopup(Res,Rep,_service));
    }
}

using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.ViewModels;
using CommunityToolkit.Maui.Alerts;
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

    private async void TapGestureRecognizer_DeleteLead(object sender, TappedEventArgs e)
    {
        bool ans = await DisplayAlert("Question", "Are you sure to delete This Lead", "Ok", "Cancel");
        if (ans)
        {
            this.IsEnabled = false;
            UserDialogs.Instance.ShowLoading();
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string res = await Rep.PostEAsync($"{ApiConstants.LeadDeleteApi}{AccId}/Lead/{Res.Id}/Delete", UserToken);
                if (res == "")
                {
                    var toast = Toast.Make($"Lead Deleted Successfully", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else
                {
                    var toast = Toast.Make($"Can't Delete This Lead Now Try ahin Later", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                await MopupService.Instance.PopAsync();
            }
            UserDialogs.Instance.HideHud();
            this.IsEnabled = true;
        }
    }

    private async void TapGestureRecognizer_ShareLead(object sender, TappedEventArgs e)
    {
        await MopupService.Instance.PopAsync();
        await MopupService.Instance.PushAsync(new ShareLeadPopup(Res,Rep,_service));
    }

    private async void TapGestureRecognizer_ShowComments(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AllCommentPage(new AllCommentViewModel(Res,Rep,_service)));
    }
}
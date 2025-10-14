using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Models.LeadComment;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Globalization;

namespace Cardrly.Pages.MainPopups;

public partial class CommentPopup : Mopups.Pages.PopupPage
{
    LeadResponse Res = new LeadResponse();

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion

    #region Cons
    public CommentPopup(LeadResponse res, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        Res = res;
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
    #endregion

    #region Methods
    private async void Save_Clicked(object sender, EventArgs e)
    {
        this.IsEnabled = false;
        try
        {
            if (string.IsNullOrEmpty(CommEntr.Text))
            {
                var toast = Toast.Make($"{AppResources.msgFRComment}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                this.IsEnabled = false;
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    UserDialogs.Instance.ShowLoading();
                    LeadCommentRequest req = new LeadCommentRequest
                    {
                        Comment = CommEntr.Text,

                    };
                    var json = await Rep.PostTRAsync<LeadCommentRequest, LeadCommentResponse>($"{ApiConstants.LeadCommentAddApi}{AccId}/Lead/{Res.Id}/LeadComment", req, UserToken);
                    UserDialogs.Instance.HideHud();
                    if (json.Item1 != null)
                    {
                        var toast = Toast.Make($"{AppResources.msgSuccessfullyAddComment}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                        await Task.Delay(1000);
                        await MopupService.Instance.PopAsync();
                    }
                    else
                    {
                        var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                        //await MopupService.Instance.PopAsync();
                    }
                }
                this.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
        this.IsEnabled = true;
    }


    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }
    #endregion

}
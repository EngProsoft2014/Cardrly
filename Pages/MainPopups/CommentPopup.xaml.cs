using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Models.LeadComment;
using CommunityToolkit.Maui.Alerts;
using Controls.UserDialogs.Maui;
using Mopups.Services;

namespace Cardrly.Pages.MainPopups;

public partial class CommentPopup : Mopups.Pages.PopupPage
{
    LeadResponse Res = new LeadResponse();
    LeadCommentResponse CommentRes = new LeadCommentResponse();
    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public CommentPopup(LeadResponse res, IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        Rep = GenericRep;
        _service = service;
        Res = res;
        //Init();
    }

    async void Init()
    {
        await GetComment();
        CommEntr.Text = CommentRes.Comment;
    }
    async Task GetComment()
    {
        this.IsEnabled = false;
        string UserToken = await _service.UserToken();
        if (!string.IsNullOrEmpty(UserToken))
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            UserDialogs.Instance.ShowLoading();
            var json = await Rep.GetAsync<LeadCommentResponse>($"{ApiConstants.LeadCommentGetAllApi}{AccId}/Lead/{Res.Id}/LeadComment", UserToken);

            if (json != null)
            {
                CommentRes = json;
            }
        }
        this.IsEnabled = true;
        UserDialogs.Instance.HideHud();
    }

    private async void Save_Clicked(object sender, EventArgs e)
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
            var json =  await Rep.PostTRAsync<LeadCommentRequest,LeadCommentResponse>($"{ApiConstants.LeadCommentAddApi}{AccId}/Lead/{Res.Id}/LeadComment",req, UserToken);
            if (json.Item1 != null)
            {
                var toast = Toast.Make("Successfully Add Comment.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
                await MopupService.Instance.PopAsync();
            }
            else
            {
                var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
                await MopupService.Instance.PopAsync();
            }
            UserDialogs.Instance.HideHud();
            
        }
        this.IsEnabled = true;
    }

    private async void Cancel_Clicked(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }
}
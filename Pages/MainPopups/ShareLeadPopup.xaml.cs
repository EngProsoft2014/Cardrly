using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Models.LeadAssign;
using CommunityToolkit.Maui.Alerts;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Collections.ObjectModel;

namespace Cardrly.Pages.MainPopups;

public partial class ShareLeadPopup : Mopups.Pages.PopupPage
{
    LeadResponse leadResponse = new LeadResponse();
    ObservableCollection<LeadAssignResponse> leadAssignResponses = new ObservableCollection<LeadAssignResponse>();

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion

    public ShareLeadPopup(LeadResponse res,IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        Rep = GenericRep;
        _service = service;
        leadResponse = res;
        Init();
    }


    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    #region Methodes
    public async void Init()
    {
        await GetAllUsers();
        UserColc.ItemsSource = leadAssignResponses;
    }

    async Task GetAllUsers()
    {
        this.IsEnabled = false;
        string UserToken = await _service.UserToken();
        if (!string.IsNullOrEmpty(UserToken))
        {
            UserDialogs.Instance.ShowLoading();
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            var json = await Rep.GetAsync<ObservableCollection<LeadAssignResponse>>($"{ApiConstants.LeadAssignGetAllApi}{AccId}/Lead/{leadResponse.Id}/LeadAssign", UserToken);
            if (json != null)
            {
                foreach (LeadAssignResponse res in json)
                {
                    res.IsAdded = !string.IsNullOrEmpty(res.Id);
                }
                leadAssignResponses = json;
            }
        }
        UserDialogs.Instance.HideHud();
        this.IsEnabled = true;
    }
    #endregion

    private async void IsShareToUsers_Tapped(object sender, TappedEventArgs e)
    {
        var Item = e.Parameter as LeadAssignResponse;
        if (!string.IsNullOrEmpty(Item.Id))
        {
            await Remove(Item.Id!);
        }
        else
        {
            await Add(Item.CardId!);
        }
    }

    private async void AllowChangeIsShareToUsers_Tapped(object sender, TappedEventArgs e)
    {
        var Item = e.Parameter as LeadAssignResponse;
        string UserToken = await _service.UserToken();
        if (!string.IsNullOrEmpty(UserToken))
        {
            this.IsEnabled = false;
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            UserDialogs.Instance.ShowLoading();
            string ressponse = await Rep.PostEAsync($"{ApiConstants.CardLinkToggleApi}{AccId}/Lead/{leadResponse.Id}/LeadAssign/{Item.Id}/ToggleActive", UserToken);
            if (ressponse == "")
            {
                Init();
            }
            else
            {
                var toast = Toast.Make($"Failed to change status", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            this.IsEnabled = true;
            UserDialogs.Instance.HideHud();
        }
    }

    public async Task Add(string LeadId)
    {
        string UserToken = await _service.UserToken();
        if (!string.IsNullOrEmpty(UserToken))
        {
            this.IsEnabled = false;
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            UserDialogs.Instance.ShowLoading();
            LeadAssignRequest assignRequest = new LeadAssignRequest() { CardId = LeadId};
            var ressponse = await Rep.PostTRAsync<LeadAssignRequest,LeadAssignResponse>($"{ApiConstants.LeadAssignToggleApi}{AccId}/Lead/{leadResponse.Id}/LeadAssign",assignRequest, UserToken);
            if (ressponse.Item1 != null)
            {
                Init();
            }
            else
            {
                var toast = Toast.Make($"Failed to change status", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            this.IsEnabled = true;
            UserDialogs.Instance.HideHud();
        }
    }

    public async Task Remove(string LeadAssignId)
    {
        string UserToken = await _service.UserToken();
        if (!string.IsNullOrEmpty(UserToken))
        {
            this.IsEnabled = false;
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            UserDialogs.Instance.ShowLoading();
            string ressponse = await Rep.PostEAsync($"{ApiConstants.CardLinkToggleApi}{AccId}/Lead/{leadResponse.Id}/LeadAssign/{LeadAssignId}/Delete", UserToken);
            if (ressponse == "")
            {
                Init();
            }
            else
            {
                var toast = Toast.Make($"Failed to change status", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            this.IsEnabled = true;
            UserDialogs.Instance.HideHud();
        }
    }
}
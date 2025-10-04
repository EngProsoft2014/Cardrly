
using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Models.Card;
using Cardrly.Models.Lead;
using Cardrly.Pages;
using Cardrly.Pages.MainPopups;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels.Leads
{
    public partial class LeadViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        ObservableCollection<LeadResponse> leads = new ObservableCollection<LeadResponse>();
        [ObservableProperty]
        ObservableCollection<LeadResponse> leadsInPage = new ObservableCollection<LeadResponse>();
        [ObservableProperty]
        public LeadFilterRequest filterRequest = new LeadFilterRequest();
        [ObservableProperty]
        PagingLstLeadResponse pagingResponse = new PagingLstLeadResponse();
        [ObservableProperty]
        CardDetailsResponse myCardDetails = new CardDetailsResponse();

        public bool IsHasNext { get; set; }

        int OldPageNumber = 0; // As inactive changed check last page number
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public LeadViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task AddLeadClick()
        {
            if (StaticMember.CheckPermission(ApiConstants.AddLeads))
            {
                await GetAccountCard();
                if (!string.IsNullOrEmpty(MyCardDetails.Id))
                {
                    await App.Current!.MainPage!.Navigation.PushAsync(new AddLeadsPage(new AddLeadViewModel(Rep, _service)));
                }
                else
                {
                    var toast = Toast.Make($"{AppResources.msgPleaseCreateCardFirst_}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }
            else
            {
                var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        [RelayCommand]
        async Task ActiveClick(LeadResponse lead)
        {
            if (StaticMember.CheckPermission(ApiConstants.UpdateLeads))
            {
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    UserDialogs.Instance.ShowLoading();
                    string ressponse = await Rep.PostEAsync($"{ApiConstants.LeadToggleApi}{AccId}/Lead/{lead.Id}/ToggleActive", UserToken);
                    if (ressponse == "")
                    {
                        //FilterRequest = new LeadFilterRequest();
                        OldPageNumber = FilterRequest.PageNumber!.Value - 1;
                        FilterRequest.PageNumber = 1;
                        FilterRequest.Pagesize = OldPageNumber != 0 ? 25 * OldPageNumber : 25;
                        await SearchLeads();
                    }
                    else
                    {
                        var toast = Toast.Make($"{AppResources.msgFailedToChangeStatus}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                    UserDialogs.Instance.HideHud();
                }
            }
            else
            {
                var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        [RelayCommand]
        async Task SelectClick(LeadResponse lead)
        {
            if (StaticMember.CheckPermission(ApiConstants.UpdateLeads))
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new AddLeadsPage(new AddLeadViewModel(lead, Rep, _service)));
            }
            else
            {
                var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }

        }
        [RelayCommand]
        async Task MoreOptionClick(LeadResponse res)
        {
            await MopupService.Instance.PushAsync(new LeadOptionsPopup(res, Rep, _service));
        }
        [RelayCommand]
        async Task GetLoadMore()
        {
            if (IsHasNext && string.IsNullOrEmpty(FilterRequest.SearchLead))
            {
                await GetAllLeads();
            }
        }

        [RelayCommand]
        public async Task FilterClick()
        {
            if (StaticMember.CheckPermission(ApiConstants.GetLeads))
            {
                var page = new LeadFilterPopup(FilterRequest,Rep,_service);
                page.FilterClose += async (filter) =>
                {
                    await MopupService.Instance.PopAsync();
                    IsHasNext = false;
                    FilterRequest = filter;
                    Leads = new ObservableCollection<LeadResponse>();
                    await GetAllLeads();
                };
                await MopupService.Instance.PushAsync(page);
            }
            else
            {
                var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }

        }
        #endregion

        #region Methodes
        public async void Init()
        {
            IsHasNext = true;
            if (StaticMember.CheckPermission(ApiConstants.GetLeads))
            {
                await GetAllLeads();
            }
            else
            {
                var toast = Toast.Make($"{AppResources.mshPermissionToViewData}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            MessagingCenter.Subscribe<LeadOptionsPopup, bool>(this, "DeleteLead", async (sender, message) =>
            {

                if (true)
                {
                    FilterRequest = new LeadFilterRequest();
                    await SearchLeads();
                }
            });
            MessagingCenter.Subscribe<AddLeadViewModel, bool>(this, "CreateLead", async (sender, message) =>
            {

                if (true)
                {
                    FilterRequest.PageNumber = 1;
                    FilterRequest.SearchLead = "";
                    await SearchLeads();
                }
            });
        }
        public async Task GetAllLeads()
        {
            UserDialogs.Instance.ShowLoading();
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

                string Query = QueryStringHelper.ToQueryString(FilterRequest);

                var json = await Rep.GetAsync<PagingLstLeadResponse>($"{ApiConstants.LeadGetAllApi}{AccId}/Lead/Page?{Query}", UserToken);

                if (json != null)
                {
                    PagingResponse = json;

                    //IsHasNext = json.pagingLst.HasNextPages;

                    IsHasNext = PagingResponse.pagingLst.DataModel.Count > 0 ? true : false;

                    LeadsInPage = new ObservableCollection<LeadResponse>(PagingResponse?.pagingLst.DataModel!);

                    if (Leads.Count == 0)
                    {
                        Leads = new ObservableCollection<LeadResponse>(LeadsInPage.ToList());
                    }
                    else
                    {
                        if (Leads != LeadsInPage)
                        {
                            LeadsInPage.ToList().ForEach(f => Leads.Add(f));
                        }
                    }
                }

                FilterRequest.PageNumber += 1;
            }
            UserDialogs.Instance.HideHud();
        }
        public async Task SearchLeads()
        {
            if (StaticMember.CheckPermission(ApiConstants.GetLeads))
            {
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    string Query = QueryStringHelper.ToQueryString(FilterRequest);

                    var json = await Rep.GetAsync<PagingLstLeadResponse>($"{ApiConstants.LeadGetAllApi}{AccId}/Lead/Page?{Query}", UserToken);

                    if (json != null)
                    {
                        PagingResponse = json;

                        //IsHasNext = json.pagingLst.HasNextPages;

                        IsHasNext = PagingResponse.pagingLst.DataModel.Count > 0 ? true : false;

                        LeadsInPage = new ObservableCollection<LeadResponse>(PagingResponse?.pagingLst.DataModel!);

                        Leads = new ObservableCollection<LeadResponse>(LeadsInPage.ToList());
 
                        if(OldPageNumber != 0)
                        {
                            FilterRequest.PageNumber = OldPageNumber + 1;
                            FilterRequest.Pagesize = 25;
                        }
                    }
                }
            }
            else
            {
                var toast = Toast.Make($"{AppResources.mshPermissionToViewData}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
        async Task GetAccountCard()
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string UserId = Preferences.Default.Get(ApiConstants.userid, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<CardDetailsResponse>($"{ApiConstants.CardGetByUserApi}{AccId}/Card/User/{UserId}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    MyCardDetails = json;
                }
            }
        }
        #endregion
    }
}

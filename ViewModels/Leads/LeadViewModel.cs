﻿
using Cardrly.Constants;
using Cardrly.Helpers;
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
        public readonly IAudioManager _audioManager;

        public bool IsHasNext { get; set; }
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public LeadViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioManager audioManager)
        {
            Rep = GenericRep;
            _service = service;
            // Initialize audio manager
            _audioManager = audioManager;
            Init();
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task AddLeadClick()
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new AddLeadsPage(new AddLeadViewModel(Rep, _service), _audioManager));
        }
        [RelayCommand]
        async Task ActiveClick(LeadResponse lead)
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                IsEnable = false;
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                string ressponse = await Rep.PostEAsync($"{ApiConstants.LeadToggleApi}{AccId}/Lead/{lead.Id}/ToggleActive", UserToken);
                if (ressponse == "")
                {
                    Init();
                }
                else
                {
                    var toast = Toast.Make($"{AppResources.msgFailedToChangeStatus}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                IsEnable = true;
                UserDialogs.Instance.HideHud();
            }
        }

        [RelayCommand]
        async Task SelectClick(LeadResponse lead)
        {
            await App.Current!.MainPage!.Navigation.PushAsync(new AddLeadsPage(new AddLeadViewModel(lead, Rep, _service), _audioManager));
        }

        [RelayCommand]
        async Task MoreOptionClick(LeadResponse res)
        {
            await MopupService.Instance.PushAsync(new LeadOptionsPopup(res, Rep, _service));
        }
        #endregion

        #region Methodes
        public async void Init()
        {
            IsHasNext = true;
            await GetAllLeads();
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
                }
            }
        }
        [RelayCommand]
        async Task GetLoadMore()
        {
            if (IsHasNext)
            {
                await GetAllLeads();
            }
        }
        #endregion
    }
}

﻿
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Pages;
using Cardrly.Pages.MainPopups;
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
        [ObservableProperty]
        ObservableCollection<LeadResponse> leads = new ObservableCollection<LeadResponse>();
        public readonly IAudioManager _audioManager;
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
                    var toast = Toast.Make($"Failed to change status", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
            await MopupService.Instance.PushAsync(new LeadOptionsPopup(res,Rep,_service));
        }
        #endregion

        #region Methodes
        public async void Init()
        {
            await GetAllLeads();
        }

        async Task GetAllLeads()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<ObservableCollection<LeadResponse>>($"{ApiConstants.LeadGetAllApi}{AccId}/Lead", UserToken);
                
                if (json != null)
                {
                    foreach (LeadResponse Lead in json)
                    {
                        if (!string.IsNullOrEmpty(Lead.UrlImgProfile))
                        {
                            Lead.UrlImgProfile = Utility.ServerUrl + Lead.UrlImgProfile;
                        }
                    }
                    Leads = json;
                }
            }
            IsEnable = true;
            UserDialogs.Instance.HideHud();
        }
        #endregion
    }
}

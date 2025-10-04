
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Models.LeadComment;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class AllCommentViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        LeadResponse res = new LeadResponse();
        [ObservableProperty]
        ObservableCollection<LeadCommentResponse> leadComments = new ObservableCollection<LeadCommentResponse>();
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public AllCommentViewModel(LeadResponse leadResponse,IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Res = leadResponse;
            Init();
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task DeleteClick(LeadCommentResponse leadComment)
        {
            bool result = await App.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgDeleteComment}", $"{AppResources.msgYes}", $"{AppResources.msgNo}");
            if (result)
            {
                IsEnable = false;
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    UserDialogs.Instance.ShowLoading();
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    string response = await Rep.PostEAsync($"{ApiConstants.LeadCommentDeleteApi}{AccId}/Lead/{leadComment.LeadId}/LeadComment/{leadComment.Id}/Delete", UserToken);
                    UserDialogs.Instance.HideHud();
                    if (response == "")
                    {
                        UserDialogs.Instance.ShowLoading();
                        await GetComment();
                        UserDialogs.Instance.HideHud();
                    }
                    else
                    {
                        var toast = Toast.Make($"{AppResources.msgThe_comment_has_not_been_deleted}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                IsEnable = true;
            }
        } 
        #endregion

        #region Method
        async void Init()
        {
            await MopupService.Instance.PopAsync();
            await GetComment();
        }
        async Task GetComment()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<ObservableCollection<LeadCommentResponse>>($"{ApiConstants.LeadCommentGetAllApi}{AccId}/Lead/{Res.Id}/LeadComment", UserToken);
                if (json != null)
                {
                    LeadComments = json;
                }
                UserDialogs.Instance.HideHud();
            }
            IsEnable = true;
        } 
        #endregion
    }
}


using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Models.LeadComment;
using CommunityToolkit.Mvvm.ComponentModel;
using Controls.UserDialogs.Maui;
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

        #region Method
        async void Init()
        {
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
            }
            IsEnable = true;
            UserDialogs.Instance.HideHud();
        } 
        #endregion
    }
}

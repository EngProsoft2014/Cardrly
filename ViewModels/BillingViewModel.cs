
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Billing;
using CommunityToolkit.Mvvm.ComponentModel;
using Controls.UserDialogs.Maui;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class BillingViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        public CustomerPaymentDetailsModel paymentDetailsModels = new CustomerPaymentDetailsModel();
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public BillingViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        }
        #endregion

        #region Methods
        async void Init()
        {
            await GetAccountCard();
        }
        async Task GetAccountCard()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<CustomerPaymentDetailsModel>($"{ApiConstants.StripeGetBillingApi}{AccId}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    PaymentDetailsModels = json;
                }
            }
            IsEnable = true;
        }
        #endregion
    }
}

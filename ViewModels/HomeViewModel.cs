﻿using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Mode_s.Account;
using Cardrly.Models.Card;
using Cardrly.Models.Home;
using Cardrly.Models.Permision;
using Cardrly.Pages.MainPopups;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Newtonsoft.Json;
//using Plugin.Firebase.CloudMessaging;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace Cardrly.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        CardDashBoardResponse boardResponse = new CardDashBoardResponse();
        [ObservableProperty]
        AccountResponse accResponse = new AccountResponse();
        [ObservableProperty]
        public ObservableCollection<CardResponse> cardLst = new ObservableCollection<CardResponse>();
        [ObservableProperty]
        CardResponse selectedCard = new CardResponse();
        [ObservableProperty]
        DateTime fromDate = DateTime.UtcNow.Date;
        [ObservableProperty]
        DateTime toDate = DateTime.UtcNow.Date;
        [ObservableProperty]
        bool isFoundCards = false;
        [ObservableProperty]
        int isCheckOrGo = 1; // 1: check, 2: go;
        [ObservableProperty]
        bool _isAnimating = false; // Flag to control animation
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public HomeViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            LoadPermissions();
            Rep = GenericRep;
            _service = service;
            Init();
            
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task GetClick()
        {
            if (FromDate.Date >ToDate.Date)
            {
                var toast = Toast.Make($"{AppResources.msgDate_HomePage}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                UserDialogs.Instance.ShowLoading();
                await GetAllStatistics();
                IsCheckOrGo = 1;
                UserDialogs.Instance.HideHud();
            }

            // await MopupService.Instance.PushAsync(new UpdateVersionPopup());

            ////get token for test
            //await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            //var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            //await Share.RequestAsync(token);

        }

        #endregion

        #region Methodes
        public async void Init()
        {
            SelectedCard = new CardResponse() { CardName = "NoCard"};
            await GetAllCards();
            if (CardLst.Count > 0)
            {
                SelectedCard = CardLst[0];
                IsFoundCards = true;
            }
            UserDialogs.Instance.ShowLoading();
            await Task.WhenAll(GetAllStatistics(),GetAccData());
            UserDialogs.Instance.HideHud();
        }

        async Task GetAllStatistics()
        {
            IsEnable = false;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

                    var json = await Rep.GetAsync<CardDashBoardResponse>($"{ApiConstants.CardGetCardDashBoardApi}{AccId}/Card/{SelectedCard.Id}/{FromDate.ToString("yyyy-MM-dd")}/{ToDate.ToString("yyyy-MM-dd")}", UserToken);

                    if (json != null)
                    {
                        if (json.clickCardLinkSummariesOS.Count == 0)
                        {
                            json.clickCardLinkSummariesOS.Add(new ClickCardSummaryOSResponse { name = "Empty", value = 1 });
                        }
                        if (json.clickCardSummariesCountry.Count == 0)
                        {
                            json.clickCardSummariesCountry.Add(new ClickCardSummaryCountryResponse { name = "Empty", value = 1 });
                        }
                        if (json.clickCardLinkSummariesCountry.Count == 0)
                        {
                            json.clickCardLinkSummariesCountry.Add(new ClickCardLinkSummaryCountryResponse { name = "Empty", value = 1 });
                        }
                        BoardResponse = json;
                    }
                }
                IsCheckOrGo = 1;
                IsAnimating = false;
            }
            IsEnable = true;
        }

        public async Task GetAccData()
        {
            IsEnable = false;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

                    var json = await Rep.GetAsync<AccountResponse>($"{ApiConstants.AccountInfoApi}{AccId}", UserToken);

                    if (json != null)
                    {
                        AccResponse = json;
                        AccResponse.CardProgress = ((json.CurrentCountCards / (double)json.CountCards) * 100);
                        AccResponse.UsersProgress = Convert.ToInt32((json.CurrentCountUsers / (double)json.CountUsers) * 100);
                        AccResponse.ExpireProgress = ((json.DayOperationAcc - json.DayOperationExpireAcc) / (double)json.DayOperationAcc) * 100;
                        AccResponse.RemmingDays = json.DayOperationAcc - json.DayOperationExpireAcc;
                    }
                }
                IsCheckOrGo = 1;
                IsAnimating = false;
            }
            IsEnable = true;
        }

        public async Task GetAllCards()
        {
            IsEnable = false;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");

                    var json = await Rep.GetAsync<ObservableCollection<CardResponse>>($"{ApiConstants.CardGetAllApi}{AccId}/Card", UserToken);

                    if (json != null && json.Count > 0)
                    {
                        CardLst = json;
                    }
                }
                IsCheckOrGo = 1;
                IsAnimating = false;
            }
            IsEnable = true;
        }

        void LoadPermissions()
        {
            string List = Preferences.Default.Get(ApiConstants.userPermision, "");
            if (!string.IsNullOrEmpty(List))
            {
                StaticMember.LstPermissions = JsonConvert.DeserializeObject<List<PermissionsValues>>(List)!;
            }
        }
        #endregion
    }
}

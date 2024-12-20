﻿
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Pages.MainPopups;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Microsoft.AspNet.SignalR.Client.Http;
using Mopups.Services;

namespace Cardrly.ViewModels.Leads
{
    public partial class AddLeadViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        public LeadRequest? request = new LeadRequest();
        [ObservableProperty]
        LeadResponse ? response = new LeadResponse();
        [ObservableProperty]
        public int isProfileImageAdded = 1;
        [ObservableProperty]
        int addOrUpdate = 0; // 1 - Add & 2 - update  
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public AddLeadViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            AddOrUpdate = 1;
        }

        public AddLeadViewModel(LeadResponse leadResponse, IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            AddOrUpdate = 2;
            Response = leadResponse;
            Init(leadResponse);
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task AddProfileImageClick()
        {
            var page = new AddAttachmentsPopup();
            page.ImageClose += async (img, imgPath) =>
            {
                if (!string.IsNullOrEmpty(img))
                {
                    byte[] bytes = Convert.FromBase64String(img);
                    Request.ImgFile = bytes;
                    Request.ImagefileProfile = ImageSource.FromStream(() => new MemoryStream(bytes));
                    Request.Extension = Path.GetExtension(imgPath);
                    IsProfileImageAdded = 2;
                    await MopupService.Instance.PopAsync();
                }
            };
            await MopupService.Instance.PushAsync(page);
        }

        [RelayCommand]
        async Task SaveClick()
        {
            IsEnable = false;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                UserDialogs.Instance.ShowLoading();
                string UserToken = await _service.UserToken();
                string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                if (AddOrUpdate == 1)
                {
                    if (AddOrUpdate == 1)
                    {
                        LeadRequestDto requestDto = new LeadRequestDto
                        {
                            FullName = Request!.FullName,
                            Phone = Request.Phone,
                            Address = Request.Address,
                            Company = Request.Company,
                            Extension = Request.Extension,
                            Email = Request.Email,
                            ImgFile = Request.ImgFile,
                            Website = Request.Website
                        };
                        var json = await Rep.PostTRAsync<LeadRequestDto, LeadResponse>($"{ApiConstants.LeadAddApi}{accid}/Lead", requestDto, UserToken);
                        
                        if (json.Item1 != null)
                        {
                            var toast = Toast.Make("Successfully Add Lead.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();

                        }
                        else if (json.Item2 != null)
                        {
                            var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                        UserDialogs.Instance.HideHud();
                    }
                    else if (AddOrUpdate == 2)
                    {
                        LeadRequestDto requestDto = new LeadRequestDto
                        {
                            FullName = Request!.FullName,
                            Phone = Request.Phone,
                            Address = Request.Address,
                            Company = Request.Company,
                            Extension = Request.Extension,
                            Email = Request.Email,
                            ImgFile = Request.ImgFile,
                            Website = Request.Website
                        };
                        var json = await Rep.PostTRAsync<LeadRequestDto, LeadResponse>($"{ApiConstants.LeadAddApi}{accid}/Lead/{Response.Id}", requestDto, UserToken);
                        
                        if (json.Item1 != null)
                        {
                            var toast = Toast.Make("Successfully Upadte Lead.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();

                        }
                        else if (json.Item2 != null)
                        {
                            var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                        UserDialogs.Instance.HideHud();     
                    }
                    
                }
                else if (AddOrUpdate == 2)
                {
                    LeadRequestDto requestDto = new LeadRequestDto
                    {
                        FullName = Request!.FullName,
                        Phone = Request.Phone,
                        Address = Request.Address,
                        Company = Request.Company,
                        Extension = Request.Extension,
                        Email = Request.Email,
                        ImgFile = Request.ImgFile,
                        Website = Request.Website
                    };
                    var json = await Rep.PostTRAsync<LeadRequestDto, LeadRequest>($"{ApiConstants.CardUpdateApi}{accid}/Card/", requestDto, UserToken);
                    UserDialogs.Instance.HideHud();
                    if (json.Item1 != null)
                    {
                        var toast = Toast.Make("Successfully update Lead.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();

                    }
                    else if (json.Item2 != null)
                    {
                        var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }

            }
            UserDialogs.Instance.HideHud();
            IsEnable = true;
        }
        #endregion

        #region Mehtods
        void Init(LeadResponse lead)
        {
            Request.Website = lead.Website;
            Request.Address = lead.Address;
            Request.Company = lead.Company;
            Request.Email = lead.Email;
            Request.FullName = lead.FullName;
            Request.Phone = lead.Phone;
            Request.ImagefileProfile = ImageSource.FromUri(new Uri(lead.UrlImgProfile));
        } 
        #endregion
    }
}

using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Models.LeadCategory;
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
using System.Text.RegularExpressions;

namespace Cardrly.ViewModels.Leads
{
    public partial class AddLeadViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        public LeadRequest? request = new LeadRequest();
        [ObservableProperty]
        public LeadScanCardRequest? scanCard = new LeadScanCardRequest();
        [ObservableProperty]
        LeadResponse? response = new LeadResponse();
        [ObservableProperty]
        public int isProfileImageAdded = 1;
        [ObservableProperty]
        public bool isNotificationVM = false;
        [ObservableProperty]
        int addOrUpdate = 0; // 1 - Add & 2 - update  
        [ObservableProperty]
        ObservableCollection<SelectListCategory> listCategories = new ObservableCollection<SelectListCategory>();
        [ObservableProperty]
        SelectListCategory selectedLeadCategory = new SelectListCategory();
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
            Request.ImagefileProfile = ImageSource.FromFile("usericon.png");
            Init();
        }

        public AddLeadViewModel(LeadResponse leadResponse, IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            IsNotificationVM = leadResponse.IsNotification;
            Response = leadResponse;
            AddOrUpdate = leadResponse.IsNotification == true ? 3 : 2; //3 for notification lead details //2 for update   
            Init(leadResponse);
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task AddProfileImageClick()
        {
            AddAttachmentsPopup page;
            if (Request!.ImgFile != null)
            {
                page = new AddAttachmentsPopup(false, Request.ImgFile);
            }
            else if (!string.IsNullOrEmpty(Response!.UrlImgProfileVM) & Response.UrlImgProfileVM != "usericon.png")
            {
                UserDialogs.Instance.ShowLoading($"{AppResources.msgLoadingImage}");
                var bytes = await StaticMember.GetImageBase64FromUrlAsync(Response.UrlImgProfileVM);
                UserDialogs.Instance.HideHud();
                page = new AddAttachmentsPopup(false, bytes);
            }
            else
            {
                page = new AddAttachmentsPopup();
            }
            page.ImageClose += (img, imgPath) =>
            {
                if (!string.IsNullOrEmpty(img))
                {
                    MopupService.Instance.PopAsync();
                    byte[] bytes = Convert.FromBase64String(img);
                    Request.ImgFile = bytes;
                    Request.ImagefileProfile = ImageSource.FromStream(() => new MemoryStream(bytes));
                    Request.Extension = Path.GetExtension(imgPath);
                    IsProfileImageAdded = 2;

                }
            };
            await MopupService.Instance.PushAsync(page);
        }

        [RelayCommand]
        async Task OpenFullScreenProfilePhoto(ImageSource image)
        {
            IsEnable = false;
            await App.Current!.MainPage!.Navigation.PushAsync(new FullScreenImage(image));
            IsEnable = true;
        }


        [RelayCommand]
        async Task SaveClick()
        {
            string valid = "";
            if (!string.IsNullOrEmpty(Request.Email))
            {
                valid = CheckStringType(Request.Email);
            }

            if (string.IsNullOrEmpty(Request!.FullName))
            {
                var toast = Toast.Make($"{AppResources.msgFRFullName}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else if (!string.IsNullOrEmpty(Request.Email) && valid != "Email")
            {
                var toast = Toast.Make($"{AppResources.msgCheck_your_email_and_reEnter_it_correctly}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                IsEnable = false;
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {

                    string UserToken = await _service.UserToken();
                    string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                    LeadRequestDto requestDto = new LeadRequestDto
                    {
                        FullName = Request!.FullName,
                        Phone = Request.Phone,
                        JobTitle = Request.JobTitle,
                        Address = Request.Address,
                        Company = Request.Company,
                        Extension = Request.Extension,
                        Email = Request.Email,
                        ImgFile = Request.ImgFile,
                        Website = Request.Website,
                        LeadCategoryId = SelectedLeadCategory?.Value
                    };
                    (string, ErrorResult) json = ("a", new ErrorResult());
                    if (AddOrUpdate == 1)//Add
                    {
                        UserDialogs.Instance.ShowLoading();
                        json = await Rep.PostTRAsync<LeadRequestDto, string>($"{ApiConstants.LeadAddApi}{accid}/Lead", requestDto, UserToken);
                        UserDialogs.Instance.HideHud();
                        if (json.Item1 == null && json.Item2 == null)
                        {
                            var toast = Toast.Make($"{AppResources.msgSuccessfullyAddLead}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                            MessagingCenter.Send(this, "CreateLead", true);
                            await App.Current!.MainPage!.Navigation.PopAsync();
                        }
                        else if (json.Item2 != null)
                        {
                            var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                    }
                    else if (AddOrUpdate == 2)//Update
                    {
                        UserDialogs.Instance.ShowLoading();
                        var json2 = await Rep.PostTRAsync<LeadRequestDto, LeadResponse>($"{ApiConstants.LeadUpdateApi}{accid}/Lead/{Response.Id}", requestDto, UserToken);
                        UserDialogs.Instance.HideHud();
                        if (json.Item1 != null)
                        {
                            var toast = Toast.Make($"{AppResources.msgSuccessfullyUpdateLead}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                            MessagingCenter.Send(this, "CreateLead", true);
                        }
                        else if (json.Item2 != null)
                        {
                            var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                    }

                }
                IsEnable = true;
            }
        }

        [RelayCommand]
        async Task ScanClick()
        {
            if (StaticMember.CheckPermission(ApiConstants.ScanCardLeads))
            {
                AddAttachmentsPopup page;
                if (ScanCard!.ImgFile != null)
                {
                    page = new AddAttachmentsPopup(true, Request.ImgFile);
                }
                else
                {
                    page = new AddAttachmentsPopup(true);
                }
                page.ImageClose += async (img, imgPath) =>
                {
                    if (!string.IsNullOrEmpty(img))
                    {
                        await MopupService.Instance.PopAsync();

                        ScanCard.ImgFile = Convert.FromBase64String(img);

                        await UploadScanCard();

                    }
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

        #region Mehtods
        async void Init()
        {
            await GetAllCategories();
        }
        async void Init(LeadResponse lead)
        {
            await GetAllCategories();
            Request.Website = lead.Website;
            Request.Address = lead.Address;
            Request.Company = lead.Company;
            Request.Email = lead.Email;
            Request.JobTitle = lead.JobTitle;
            Request.FullName = lead.FullName;
            Request.Phone = lead.Phone;
            if (ListCategories.Count > 0 && !string.IsNullOrEmpty(lead.LeadCategoryId))
            {
                SelectedLeadCategory = ListCategories.FirstOrDefault(i => i.Value == lead.LeadCategoryId)!;
            }
            else
            {
                SelectedLeadCategory = new SelectListCategory();
            }
            if (!string.IsNullOrEmpty(lead.UrlImgProfileVM) && lead.UrlImgProfileVM != "usericon.png")
            {
                IsProfileImageAdded = 2;
                Request.ImagefileProfile = ImageSource.FromUri(new Uri(lead.UrlImgProfileVM));
            }
            else
            {
                Request.ImagefileProfile = ImageSource.FromFile("usericon.png");
            }
        }

        async Task UploadScanCard()
        {

            IsEnable = false;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {

                string UserToken = await _service.UserToken();
                string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.PostTRAsync<LeadScanCardRequest, LeadResponse>($"{ApiConstants.LeadGetScanCardApi}{accid}/Lead/GetScanCard", ScanCard, UserToken);
                UserDialogs.Instance.HideHud();
                if (json.Item1 != null)
                {
                    var toast = Toast.Make($"{AppResources.msgSuccessfullyScanCard}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                    Init(json.Item1);
                }
                else if (json.Item2 != null)
                {
                    var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }

            IsEnable = true;

        }

        async Task GetAllCategories()
        {
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<ObservableCollection<SelectListCategory>>($"{ApiConstants.LeadCategoryCurrentApi}{AccId}/LeadCategory/current", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    ListCategories = json;
                }
            }
        }

        public string CheckStringType(string input)
        {
            // Email pattern
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (Regex.IsMatch(input, emailPattern))
            {
                return "Email";
            }
            else
            {
                return "Unknown";
            }
        }
        #endregion
    }
}

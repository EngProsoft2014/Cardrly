using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Pages.MainPopups;

namespace Cardrly.ViewModels
{
    public partial class AddCustomCardViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        public CardRequest request = new CardRequest();
        [ObservableProperty]
        public int isProfileImageAdded = 1;
        [ObservableProperty]
        public int isCoverImageAdded = 1;
        [ObservableProperty]
        CardResponse card = new CardResponse();
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public AddCustomCardViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
        }
        public AddCustomCardViewModel(CardResponse _card, IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init(_card);
        }
        #endregion

        #region Methods
        void Init(CardResponse card)
        {
            Request.CardName = card.CardName;
            Request.Cardlayout = card.Cardlayout;
            Request.Bio = card.Bio;
            Request.location = card.location;
            Request.CardTheme = card.CardTheme;
            Request.FontStyle = card.FontStyle;
            Request.JobTitle = card.JobTitle;
            Request.PersonName = card.PersonName;
            Request.LinkColor = card.LinkColor;
            Request.PersonNikeName = card.PersonNikeName;
            Request.ImgProfileFile = ImageSource.FromUri(new Uri(card.UrlImgProfile!));
            IsProfileImageAdded = 2;
            Request.ImgCoverFile = ImageSource.FromUri(new Uri(card.UrlImgCover!));
            IsCoverImageAdded = 2;
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
                    Request.ImgFileProfile = bytes;
                    Request.ImgProfileFile = ImageSource.FromStream(() => new MemoryStream(bytes));
                    Request.ExtensionProfile = Path.GetExtension(imgPath);
                    IsProfileImageAdded = 2;
                    await MopupService.Instance.PopAsync();
                }
            };
            await MopupService.Instance.PushAsync(page);
        }
        [RelayCommand]
        async Task AddCoverImageClick()
        {
            var page = new AddAttachmentsPopup();
            page.ImageClose += async (img, imgPath) =>
            {
                if (!string.IsNullOrEmpty(img))
                {
                    byte[] bytes = Convert.FromBase64String(img);
                    Request.ImgFileCover = bytes;
                    Request.ImgCoverFile = ImageSource.FromStream(() => new MemoryStream(bytes));
                    Request.ExtensionCover = Path.GetExtension(imgPath);
                    IsCoverImageAdded = 2;
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
                string UserToken = await _service.UserToken();
                string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.PostTRAsync<CardRequest, CardResponse>($"{ApiConstants.CardUpdateApi}{accid}/Card/{Card.Id}" , Request, UserToken );
                UserDialogs.Instance.HideHud();
                if (json.Item1 != null)
                {
                    var toast = Toast.Make("Successfully update card.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();

                }
                else
                {
                    var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }


            IsEnable = true;
        }
        #endregion
    }
}

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.Pages.MainPopups;
using Cardrly.Models.Card;
using Cardrly.Pages.Links;
using Cardrly.ViewModels.Links;
using Cardrly.Controls;

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
        [ObservableProperty]
        int addOrUpdate = 0; // 1 - Add & 2 - update 
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
            AddOrUpdate = 1;
        }
        public AddCustomCardViewModel(CardResponse _card, IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Card = _card;
            Init(_card);
            AddOrUpdate = 2;
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
            AddAttachmentsPopup page;
            if (Request.ImgFileProfile != null)
            {
                 page = new AddAttachmentsPopup(Request.ImgFileProfile);
            }
            else if (!string.IsNullOrEmpty(Card.UrlImgProfile))
            {
                UserDialogs.Instance.ShowLoading("Loading Image");
                var bytes = await StaticMember.GetImageBase64FromUrlAsync(Card.UrlImgProfile);
                UserDialogs.Instance.HideHud();
                page = new AddAttachmentsPopup(bytes);
            }
            else
            {
                page = new AddAttachmentsPopup();
            }
            page.ImageClose += async (img, imgPath) =>
            {
                if (!string.IsNullOrEmpty(img))
                {
                    byte[] bytes = Convert.FromBase64String(img);
                    Request.ImgFileProfile = bytes;
                    Request.ImgProfileFile = ImageSource.FromStream(() => new MemoryStream(bytes));
                    Request.ExtensionProfile = Path.GetExtension(imgPath);
                    IsProfileImageAdded = 2;
                }
            };
            await MopupService.Instance.PushAsync(page);
        }
        [RelayCommand]
        async Task AddCoverImageClick()
        {
            AddAttachmentsPopup page;
            if (Request.ImgFileCover != null)
            {
                page = new AddAttachmentsPopup(Request.ImgFileCover);
            }
            else if (!string.IsNullOrEmpty(Card.UrlImgCover))
            {
                UserDialogs.Instance.ShowLoading("Loading Image");
                var bytes = await StaticMember.GetImageBase64FromUrlAsync(Card.UrlImgCover);
                UserDialogs.Instance.HideHud();
                page = new AddAttachmentsPopup(bytes);
            }
            else
            {
                page = new AddAttachmentsPopup();
            }
            page.ImageClose += async (img, imgPath) =>
            {
                if (!string.IsNullOrEmpty(img))
                {
                    byte[] bytes = Convert.FromBase64String(img);
                    Request.ImgFileCover = bytes;
                    Request.ImgCoverFile = ImageSource.FromStream(() => new MemoryStream(bytes));
                    Request.ExtensionCover = Path.GetExtension(imgPath);
                    IsCoverImageAdded = 2;
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
                
                if (AddOrUpdate == 1 )
                {
                    CardRequestDto requestDto = new CardRequestDto()
                    {
                        PersonName = Request.PersonName,
                        Cardlayout = Request.Cardlayout,
                        CardName = Request.CardName,
                        Bio = Request.Bio,
                        CardTheme = Request.CardTheme,
                        ExtensionCover = Request.ExtensionCover,
                        ExtensionProfile = Request.ExtensionProfile,
                        FontStyle = Request.FontStyle,
                        ImgFileCover = Request.ImgFileCover,
                        ImgFileProfile = Request.ImgFileProfile,
                        JobTitle = Request.JobTitle,
                        LinkColor = Request.LinkColor,
                        location = Request.location,
                        PersonNikeName = Request.PersonNikeName,
                        Email = Request.Email,
                        Password = Request.Password
                    };
                    var json = await Rep.PostTRAsync<CardRequestDto, CardResponse>($"{ApiConstants.CardUpdateApi}{accid}/Card", requestDto, UserToken);
                    UserDialogs.Instance.HideHud();
                    if (json.Item1 != null)
                    {
                        var toast = Toast.Make("Successfully Add card.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();

                    }
                    else if (json.Item2 != null)
                    {
                        var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                else if (AddOrUpdate == 2)
                {
                    CardRequestDto requestDto = new CardRequestDto()
                    {
                        PersonName = Request.PersonName,
                        Cardlayout = Request.Cardlayout,
                        CardName = Request.CardName,
                        Bio = Request.Bio,
                        CardTheme = Request.CardTheme,
                        ExtensionCover = Request.ExtensionCover,
                        ExtensionProfile = Request.ExtensionProfile,
                        FontStyle = Request.FontStyle,
                        ImgFileCover = Request.ImgFileCover,
                        ImgFileProfile = Request.ImgFileProfile,
                        JobTitle = Request.JobTitle,
                        LinkColor = Request.LinkColor,
                        location = Request.location,
                        PersonNikeName = Request.PersonNikeName,
                    };
                    var json = await Rep.PostTRAsync<CardRequestDto, CardResponse>($"{ApiConstants.CardUpdateApi}{accid}/Card/{Card.Id}", requestDto, UserToken);
                    UserDialogs.Instance.HideHud();
                    if (json.Item1 != null)
                    {
                        var toast = Toast.Make("Successfully update card.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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

        [RelayCommand]
        async Task EditLinksClick()
        {
            var vm = new LinksViewModel(Card.Id,Rep,_service);
            var page = new LinksPage(vm);
            page.BindingContext = vm;
            await App.Current!.MainPage!.Navigation.PushAsync( page );
        }
        #endregion
    }
}

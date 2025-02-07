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
using System.Collections.ObjectModel;
using Cardrly.Models;
using Cardrly.Resources.Lan;

namespace Cardrly.ViewModels
{
    public partial class AddCustomCardViewModel : BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        public CardRequest request = new CardRequest();
        [ObservableProperty]
        ObservableCollection<ColorModel> linkColor = new ObservableCollection<ColorModel>();
        [ObservableProperty]
        ObservableCollection<ColorModel> themColor = new ObservableCollection<ColorModel>();
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
            Init();
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
        void Init()
        {
            // Add Colors To Lists
            LinkColor = new ObservableCollection<ColorModel>() {
                new ColorModel { Name = "Purple", HexCode = "#673AB7" },
                new ColorModel { Name = "Green", HexCode = "#4CAF50" },
                new ColorModel { Name = "Red", HexCode = "#F44336" },
                new ColorModel { Name = "Amber", HexCode = "#FFC107" },
                new ColorModel { Name = "Blue", HexCode = "#2196F3" },
                new ColorModel { Name = "Navy", HexCode = "#0D47A1" },
                new ColorModel { Name = "white", HexCode = "#FFFFFF" },
            };
            ThemColor = new ObservableCollection<ColorModel>() {
                new ColorModel { Name = "Purple", HexCode = "#673AB7" },
                new ColorModel { Name = "Green", HexCode = "#4CAF50" },
                new ColorModel { Name = "Red", HexCode = "#F44336" },
                new ColorModel { Name = "Amber", HexCode = "#FFC107" },
                new ColorModel { Name = "Blue", HexCode = "#2196F3" },
                new ColorModel { Name = "Navy", HexCode = "#0D47A1" },
                new ColorModel { Name = "white", HexCode = "#FFFFFF" },
            };
            // Select Initial Link Colors
            LinkColor.FirstOrDefault()!.IsSelected = true;
            Request.LinkColor = LinkColor.FirstOrDefault()!.HexCode;
            // Select Initial Card Theme Colors
            ThemColor.LastOrDefault()!.IsSelected = true;
            Request.LinkColor = ThemColor.LastOrDefault()!.HexCode;
            //Add Defult Image
            Request.ImgProfileFile = ImageSource.FromFile("usericon.png");
            Request.ImgCoverFile = ImageSource.FromFile("defultcover.jpeg");
        }
        void Init(CardResponse card)
        {
            #region Map Data
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
            if (card.UrlImgProfileVM! == Utility.ServerUrl)
            {
                Request.ImgProfileFile = ImageSource.FromFile("usericon.png");
            }
            else
            {
                Request.ImgProfileFile = ImageSource.FromUri(new Uri(card.UrlImgProfileVM!));
            }
            if (card.UrlImgCoverVM! == Utility.ServerUrl)
            {
                Request.ImgCoverFile = ImageSource.FromFile("defultcover.jpeg");
            }
            else
            {
                Request.ImgCoverFile = ImageSource.FromUri(new Uri(card.UrlImgCoverVM!));
            }
            IsProfileImageAdded = 2;
            IsCoverImageAdded = 2;
            #endregion

            // Add Colors To Lists
            LinkColor = new ObservableCollection<ColorModel>() {
                new ColorModel { Name = "Purple", HexCode = "#673AB7" },
                new ColorModel { Name = "Green", HexCode = "#4CAF50" },
                new ColorModel { Name = "Red", HexCode = "#F44336" },
                new ColorModel { Name = "Amber", HexCode = "#FFC107" },
                new ColorModel { Name = "Blue", HexCode = "#2196F3" },
                new ColorModel { Name = "Navy", HexCode = "#0D47A1" },
                new ColorModel { Name = "white", HexCode = "#FFFFFF" },
            };
            ThemColor = new ObservableCollection<ColorModel>() {
                new ColorModel { Name = "Purple", HexCode = "#673AB7" },
                new ColorModel { Name = "Green", HexCode = "#4CAF50" },
                new ColorModel { Name = "Red", HexCode = "#F44336" },
                new ColorModel { Name = "Amber", HexCode = "#FFC107" },
                new ColorModel { Name = "Blue", HexCode = "#2196F3" },
                new ColorModel { Name = "Navy", HexCode = "#0D47A1" },
                new ColorModel { Name = "white", HexCode = "#FFFFFF" },
            };
            // Select Initial Link Colors
            LinkColor.FirstOrDefault()!.IsSelected = true;
            Request.LinkColor = LinkColor.FirstOrDefault()!.HexCode;
            // Select Initial Card Theme Colors
            ThemColor.LastOrDefault()!.IsSelected = true;
            Request.LinkColor = ThemColor.LastOrDefault()!.HexCode;
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task AddProfileImageClick()
        {
            AddAttachmentsPopup page;
            if (Request.ImgFileProfile != null)
            {
                page = new AddAttachmentsPopup(false, Request.ImgFileProfile);
            }
            else if (!string.IsNullOrEmpty(Card.UrlImgProfileVM) && Card.UrlImgProfileVM != Utility.ServerUrl)
            {
                UserDialogs.Instance.ShowLoading($"{AppResources.msgLoadingImage}");
                var bytes = await StaticMember.GetImageBase64FromUrlAsync(Card.UrlImgProfileVM);
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
                page = new AddAttachmentsPopup(false, Request.ImgFileCover);
            }
            else if (!string.IsNullOrEmpty(Card.UrlImgCoverVM) && Card.UrlImgCoverVM != Utility.ServerUrl)
            {
                UserDialogs.Instance.ShowLoading($"{AppResources.msgLoadingImage}");
                var bytes = await StaticMember.GetImageBase64FromUrlAsync(Card.UrlImgCoverVM);
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

                string UserToken = await _service.UserToken();
                string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
                if (string.IsNullOrEmpty(Request!.CardName))
                {
                    var toast = Toast.Make($"{AppResources.msgFRCardName}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else
                {
                    if (AddOrUpdate == 1)
                    {
                        if (string.IsNullOrEmpty(Request!.Email))
                        {
                            var toast = Toast.Make($"{AppResources.msgFREmail}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                        else if (string.IsNullOrEmpty(Request!.Password))
                        {
                            var toast = Toast.Make($"{AppResources.msgFRPassword}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                        else
                        {
                            UserDialogs.Instance.ShowLoading();
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
                                var toast = Toast.Make($"{AppResources.msgSuccessfullyAddCard}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                                await toast.Show();
                                MessagingCenter.Send(this, "AddCard", true);
                                await App.Current!.MainPage!.Navigation.PopAsync();

                            }
                            else if (json.Item2 != null)
                            {
                                var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                                await toast.Show();
                            }
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
                            var toast = Toast.Make($"{AppResources.msgSuccessfullyUpdateCard}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                            MessagingCenter.Send(this, "AddCard", true);
                            await App.Current!.MainPage!.Navigation.PopAsync();

                        }
                        else if (json.Item2 != null)
                        {
                            var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                    }
                }
            }
            UserDialogs.Instance.HideHud();
            IsEnable = true;
        }

        [RelayCommand]
        async Task EditLinksClick()
        {
            var vm = new LinksViewModel(Card.Id, Rep, _service);
            var page = new LinksPage(vm);
            page.BindingContext = vm;
            await App.Current!.MainPage!.Navigation.PushAsync(page);
        }

        [RelayCommand]
        void LinkColorClick(ColorModel model)
        {
            foreach (var item in LinkColor)
            {
                item.IsSelected = false;
            }
            LinkColor.FirstOrDefault(c => c.Name == model.Name)!.IsSelected = true;
            Request.LinkColor = model.HexCode;
        }

        [RelayCommand]
        void ThemeColorClick(ColorModel model)
        {
            foreach (var item in ThemColor)
            {
                item.IsSelected = false;
            }
            ThemColor.FirstOrDefault(c => c.Name == model.Name)!.IsSelected = true;
            Request.CardTheme = model.HexCode;
        }
        #endregion
    }
}

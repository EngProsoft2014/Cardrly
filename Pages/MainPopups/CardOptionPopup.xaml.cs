using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Mode_s.CardLink;
using Cardrly.Models.Card;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using Plugin.NFC;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Net.Maui;
using ZXing.QrCode.Internal;


namespace Cardrly.Pages.MainPopups;

public partial class CardOptionPopup : Mopups.Pages.PopupPage
{
    CardResponse Card = new CardResponse();
    CardDetailsResponse CardDetails = new CardDetailsResponse();
    string vCard = "";

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion

    #region Cons
    public CardOptionPopup(CardResponse _Card, CardDetailsResponse _CardDetails, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        Card = _Card;
        CardDetails = _CardDetails;
        //// Define vCard content
        //vCard = "BEGIN:VCARD\n" +
        //        "VERSION:3.0\n" +
        //        $"FN:{CardDetails.PersonName + " " + CardDetails.PersonNikeName}\n" +
        //        $"ADR:{CardDetails.location}\n" +
        //        $"URL:{CardDetails.CardUrlVM}\n" +
        //        "END:VCARD";


        // Assign the vCard content to the QR code
        //QRCodeImage.Value = CreateVCard(CardDetails);
        chkoffline.IsChecked = true;

        // Flow Direction 
        string Lan = Preferences.Default.Get("Lan", "en");

        if (Lan == "ar")
        {
            this.FlowDirection = FlowDirection.RightToLeft;
            CultureInfo.CurrentCulture = new CultureInfo("ar");
        }
        else
        {
            this.FlowDirection = FlowDirection.LeftToRight;
            CultureInfo.CurrentCulture = new CultureInfo("en");
        }
    }

    public static string EscapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\\", "\\\\") // Escape backslashes
            .Replace(";", "\\;")   // Escape semicolons
            .Replace(",", "\\,")   // Escape commas
            .Replace("\n", "\\n")  // Escape newlines
            .Replace("\r", "\\n"); // Normalize line endings
    }


    public string CreateVCard(CardDetailsResponse cardDetails)
    {
        // Base vCard info
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCARD");
        sb.AppendLine("VERSION:3.0");
        sb.AppendLine($"FN:{cardDetails.PersonName} {cardDetails.PersonNikeName}");
        sb.AppendLine($"ADR:{cardDetails.location}");
        sb.AppendLine($"URL:{cardDetails.CardUrlVM}");
        
        if(cardDetails.CardLinks != null && cardDetails.CardLinks.Count > 0)
        {
            // Loop through and add each link if it’s not empty
            int i = 1;
            foreach (var link in cardDetails.CardLinks)
            {
                if (!string.IsNullOrWhiteSpace(link.ValueOf))
                {
                    sb.AppendLine($"item{i}.URL:{link.ValueOf}");
                    sb.AppendLine($"item{i}.X-ABLabel:{link.AccountLinkTitle}");
                    i++;
                }
            }
        }

        //sb.AppendLine($"PHOTO;VALUE=URL:{cardDetails.UrlImgProfileVM}");

        sb.AppendLine("END:VCARD");

        return sb.ToString();
    }


    private async void CheckBox_ShareOfflineCard(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            string vcard = CreateVCard(CardDetails);

            // remove any accidental control characters
            vcard = vcard.Replace("\r\n", "\n");

            if (vcard.Length > 2953) // Version-40-L maximum ~3 KB
            {
                var toast = Toast.Make(
                    AppResources.msgvCardtoolongforastandardQRcode,
                    CommunityToolkit.Maui.Core.ToastDuration.Long,
                    15);
                await toast.Show();

                QRCodeImage.Value = "BEGIN:VCARD\n" +
                                    "VERSION:3.0\n" +
                                    $"FN:{CardDetails.PersonName + " " + CardDetails.PersonNikeName}\n" +
                                    $"ADR:{CardDetails.location}\n" +
                                    $"URL:{CardDetails.CardUrlVM}\n" +
                                    "END:VCARD";
            }
            else
            {
                QRCodeImage.Value = vcard;
            }   
        }
        else
        {
            // Assign the vCard content to the QR code
            QRCodeImage.Value = Card.CardUrlVM;
        }
    }
    #endregion

    #region Methods
    private async void TapGestureRecognizer_ShareCard(object sender, TappedEventArgs e)
    {
        await Share.RequestAsync(new ShareTextRequest
        {
            Uri = Card.CardUrlVM,
            Title = "Share Web Link"
        });
    }
    private async void TapGestureRecognizer_EditCaed(object sender, TappedEventArgs e)
    {
        if (StaticMember.CheckPermission(ApiConstants.UpdateCards))
        {
            this.IsEnabled = false;
            var vm = new AddCustomCardViewModel(Card, Rep, _service);
            var page = new AddCustomCardPage(vm);
            page.BindingContext = vm;
            await App.Current!.MainPage!.Navigation.PushAsync(page);
            await MopupService.Instance.PopAsync();
            this.IsEnabled = true;
        }
        else
        {
            var toast = Toast.Make(AppResources.msgPermissionToDoAction, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }

    }

    private async void TapGestureRecognizer_PreviewCard(object sender, TappedEventArgs e)
    {
        try
        {
            Uri uri = new Uri(Card.CardUrlVM!);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            var toast = Toast.Make(ex.Message, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }

    private async void TapGestureRecognizer_DeleteCard(object sender, TappedEventArgs e)
    {
        if (StaticMember.CheckPermission(ApiConstants.DeleteCards))
        {
            bool ans = await DisplayAlert(AppResources.msgWarning, AppResources.msgWarning_qu, AppResources.msgYes, AppResources.msgNo);
            if (ans)
            {
                await MopupService.Instance.PopAsync();
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    UserDialogs.Instance.ShowLoading();
                    string res = await Rep.PostEAsync($"{ApiConstants.CardDeleteApi}{AccId}/Card/{Card.Id}/Delete", UserToken);
                    if (res == "")
                    {
                        MessagingCenter.Send(this, "DeleteCard", true);
                    }
                    UserDialogs.Instance.HideHud();

                }
            }
        }
        else
        {
            var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }


    private async void TapGestureRecognizer_ShareVcard(object sender, TappedEventArgs e)
    {
        string fn = "contact.vcf";
        string vCardContent = VCardHelper.GenerateVCard(CardDetails);
        string file = Path.Combine(FileSystem.CacheDirectory, fn);

        File.WriteAllText(file, $"{vCardContent}");

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Share text file",
            File = new ShareFile(file)
        });
    }

    async Task GetCardsDetaikes()
    {
        this.IsEnabled = false;
        string UserToken = await _service.UserToken();
        if (!string.IsNullOrEmpty(UserToken))
        {
            string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
            UserDialogs.Instance.ShowLoading();
            var json = await Rep.GetAsync<CardDetailsResponse>($"{ApiConstants.CardGetDetailsAllApi}{Card.Id}", UserToken);

            if (json != null)
            {
                //foreach (CardLinkResponse cardLink in json.CardLinks)
                //{
                //    cardLink.AccountLinkUrlImgName = Utility.ServerUrl + cardLink.AccountLinkUrlImgName;
                //}
                CardDetails = json;
            }
            UserDialogs.Instance.HideHud();
        }
        this.IsEnabled = true;
    }
    #endregion

}
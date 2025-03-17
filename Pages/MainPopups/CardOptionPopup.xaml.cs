using Controls.UserDialogs.Maui;
using Mopups.Services;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.ViewModels;
using Plugin.NFC;
using System.Text;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using Cardrly.Mode_s.CardLink;
using System.Globalization;
using Cardrly.Resources.Lan;
using Cardrly.Controls;


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
    public CardOptionPopup(CardResponse _Card, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        Card = _Card;
        // Define vCard content
        vCard = "BEGIN:VCARD\n" +
                "VERSION:3.0\n" +
                $"FN:{Card.PersonName + " " + Card.PersonNikeName}\n" +
                $"ADR:{Card.location}\n" +
                $"URL:{Card.CardUrlVM}\n" +
                "END:VCARD";


        // Assign the vCard content to the QR code
        QRCodeImage.Value = vCard;
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

    private void CheckBox_ShareOfflineCard(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            vCard = "BEGIN:VCARD\n" +
                "VERSION:3.0\n" +
                $"FN:{Card.PersonName + " " + Card.PersonNikeName}\n" +
                $"ADR:{Card.location}\n" +
                $"URL:{Card.CardUrlVM}\n" +
                "END:VCARD";


            // Assign the vCard content to the QR code
            QRCodeImage.Value = vCard;
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
            var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
            var toast = Toast.Make($"{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }

    private async void TapGestureRecognizer_DeleteCard(object sender, TappedEventArgs e)
    {
        if (StaticMember.CheckPermission(ApiConstants.DeleteCards))
        {
            bool ans = await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgWarning_qu}", $"{AppResources.msgYes}", $"{AppResources.msgNo}");
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
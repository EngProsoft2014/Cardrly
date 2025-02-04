
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;
using CommunityToolkit.Maui.Alerts;
using Controls.UserDialogs.Maui;

using Mopups.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Cardrly.Pages.MainPopups;

public partial class LeadOptionsPopup : Mopups.Pages.PopupPage
{
    LeadResponse Res = new LeadResponse();
    int saveNum = 0;
    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public LeadOptionsPopup(LeadResponse res, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        Res = res;

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

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void TapGestureRecognizer_Comment(object sender, TappedEventArgs e)
    {
        await MopupService.Instance.PopAsync();
        await MopupService.Instance.PushAsync(new CommentPopup(Res,Rep,_service));
    }

    private async void TapGestureRecognizer_DeleteLead(object sender, TappedEventArgs e)
    {
        bool ans = await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgDeleteLead}", $"{AppResources.msgOk}", $"{AppResources.msgNo}");
        if (ans)
        {
            this.IsEnabled = false;
            UserDialogs.Instance.ShowLoading();
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                string res = await Rep.PostEAsync($"{ApiConstants.LeadDeleteApi}{AccId}/Lead/{Res.Id}/Delete", UserToken);
                if (res == "")
                {
                    var toast = Toast.Make($"{AppResources.msgLeadDeletedSuccessfully}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else
                {
                    var toast = Toast.Make($"{AppResources.msgCan_tDeleteThisLeadNowTryagainLater}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                await MopupService.Instance.PopAsync();
            }
            UserDialogs.Instance.HideHud();
            this.IsEnabled = true;
        }
    }

    private async void TapGestureRecognizer_ShareLead(object sender, TappedEventArgs e)
    {
        await MopupService.Instance.PopAsync();
        await MopupService.Instance.PushAsync(new ShareLeadPopup(Res,Rep,_service));
    }

    private async void TapGestureRecognizer_ShowComments(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AllCommentPage(new AllCommentViewModel(Res,Rep,_service)));
        await MopupService.Instance.PopAsync();
    }

    private async void TapGestureRecognizer_Call(object sender, TappedEventArgs e)
    {
        if (!string.IsNullOrEmpty(Res.Phone))
        {
            if (PhoneDialer.Default.IsSupported)
                PhoneDialer.Default.Open($"{Res.Phone}");
        }
        else
        {
            var toast = Toast.Make($"{AppResources.msgLeadPhone}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }

    private async void TapGestureRecognizer_Email(object sender, TappedEventArgs e)
    {
        if (!string.IsNullOrEmpty(Res.Email))
        {
            if (Email.Default.IsComposeSupported)
            {

                string subject = "";
                string body = "";
                string[] recipients = new[] { $"{Res.Email}" };

                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = new List<string>(recipients)
                };

                await Email.Default.ComposeAsync(message);
            }
        }
        else
        {
            var toast = Toast.Make($"{AppResources.msgLeadEmail}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }

    private async void SaveToContact_Tapped(object sender, TappedEventArgs e)
    {
        
        string vCardContent = @$"BEGIN:VCARD
VERSION:3.0
FN:{Res.FullName}
TITLE:{Res.JobTitle}
TEL:{Res.Phone}
EMAIL:{Res.Email}
ADR:{Res.Address}
URL:{Res.Website}
END:VCARD";

        string fileName = $"Contact{saveNum}.vcf";
        string filePath = GetDevicePath(fileName);

        // Save the file
        await File.WriteAllTextAsync(filePath, vCardContent);
        saveNum += 1;
        await Application.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}",
        $"{AppResources.msgContactsavedat} {filePath}", $"{AppResources.msgOk}");
        
    }

    private string GetDevicePath(string fileName)
    {
        string directoryPath = string.Empty;

#if ANDROID
        directoryPath = "/storage/emulated/0/Download/"; // Public Downloads folder
#elif WINDOWS
        directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif MACCATALYST
        directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#elif IOS
        directoryPath = FileSystem.AppDataDirectory; // iOS does not allow direct access
#endif

        return Path.Combine(directoryPath, fileName);
    }
}
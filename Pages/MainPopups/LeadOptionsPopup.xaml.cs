
using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
using Cardrly.Resources.Lan;
using Cardrly.Services;
using Cardrly.ViewModels;
using CommunityToolkit.Maui.Alerts;
using Controls.UserDialogs.Maui;
using Microsoft.Maui.ApplicationModel.Communication;
using Mopups.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Cardrly.Pages.MainPopups;

public partial class LeadOptionsPopup : Mopups.Pages.PopupPage
{
    string Name = Preferences.Default.Get(ApiConstants.AccountName, "");
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
        await Task.Delay(100);
        await MopupService.Instance.PushAsync(new CommentPopup(Res, Rep, _service));
    }

    private async void TapGestureRecognizer_DeleteLead(object sender, TappedEventArgs e)
    {
        if (StaticMember.CheckPermission(ApiConstants.DeleteLeads))
        {
            bool ans = await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgDeleteLead}", $"{AppResources.msgOk}", $"{AppResources.msgNo}");
            if (ans)
            {
                await MopupService.Instance.PopAsync();
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    UserDialogs.Instance.ShowLoading();
                    string res = await Rep.PostEAsync($"{ApiConstants.LeadDeleteApi}{AccId}/Lead/{Res.Id}/Delete", UserToken);
                    UserDialogs.Instance.HideHud();
                    if (res == "")
                    {
                        var toast = Toast.Make($"{AppResources.msgLeadDeletedSuccessfully}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                        MessagingCenter.Send(this, "DeleteLead", true);
                    }
                    else
                    {
                        var toast = Toast.Make($"{AppResources.msgCan_tDeleteThisLeadNowTryagainLater}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                    
                }

            }
        }
        else
        {
            var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }

    private async void TapGestureRecognizer_ShareLead(object sender, TappedEventArgs e)
    {
        await MopupService.Instance.PopAsync();
        await Task.Delay(100);
        await MopupService.Instance.PushAsync(new ShareLeadPopup(Res, Rep, _service));
    }

    private async void TapGestureRecognizer_ShowComments(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AllCommentPage(new AllCommentViewModel(Res, Rep, _service)));
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

        var saveContactService = App.Services.GetService<ISaveContact>();

        if (saveContactService != null)
        {
            await MopupService.Instance.PopAsync();
            await saveContactService.SaveContactMethod(Res);   
        }
        else
        {
            await Application.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgServicenotfound}", $"{AppResources.msgOk}");
        }
    }

    private async void TapGestureRecognizer_Invite(object sender, TappedEventArgs e)
    {
        await MopupService.Instance.PopAsync();
        await Share.RequestAsync("https://app.cardrly.com/");
    }

    private async void TapGestureRecognizer_Reminder(object sender, TappedEventArgs e)
    {
        await MopupService.Instance.PopAsync();
        var page = new ReminderPopup();
        page.ReminderClose += async (date) =>
        {
            // Scheduled send
            StaticMember.notificationManager.SendNotification($"Follow Up - {Res.FullName}", $"Hey {Name}, this is a reminder to follow up with {Res.FullName}. Tab here to view.", date.AddSeconds(5));
        };
        await MopupService.Instance.PushAsync(page);
    }
}
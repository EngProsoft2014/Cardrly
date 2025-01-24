
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Lead;
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
        bool ans = await DisplayAlert("Question", "Are you sure to delete This Lead", "Ok", "Cancel");
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
                    var toast = Toast.Make($"Lead Deleted Successfully", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
                else
                {
                    var toast = Toast.Make($"Can't Delete This Lead Now Try ahin Later", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
            var toast = Toast.Make("This lead don't have Phone Number", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
            var toast = Toast.Make("This lead don't have email", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }
}
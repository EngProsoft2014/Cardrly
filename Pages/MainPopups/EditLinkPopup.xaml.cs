using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.CardLink;
using Cardrly.Models.AccountLinks;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Globalization;
using System.Reactive.Joins;
using System.Text.RegularExpressions;

namespace Cardrly.Pages.MainPopups;

public partial class EditLinkPopup : Mopups.Pages.PopupPage
{
    //public class LinkTypeModel
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}

    CardLinkResponse CardLink = new CardLinkResponse();
    CardLinkResponse CardLinkRef = new CardLinkResponse();
    //List<LinkTypeModel> LinkType = new List<LinkTypeModel>();

    List<string> LinkType = new List<string>();
    int IsUpdat = 0; // 1 - Add & 0 - Update

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public EditLinkPopup(CardLinkResponse cardLink, IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        this.BindingContext = cardLink;
        Rep = GenericRep;
        _service = service;
        CardLink = new CardLinkResponse()
        {
            ValueOf = cardLink.ValueOf,
            CardLinkType = cardLink.CardLinkType,
            AccountLinkUrlImgName = cardLink.AccountLinkUrlImgName,
            AccountLinkTitle = cardLink.AccountLinkTitle,
        };
        CardLinkRef = cardLink;
        ValueEn.Text = CardLink.ValueOf;
        LoadData();
    }

    public EditLinkPopup(int isUpdate, AccountLinkResponse res,string cardId, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        //CardLink.AccountLinkUrlImgNameVM = res.UrlImgName;
        imgIcon.Source = res.UrlImgNameVM;
        CardLink.AccountLinkTitle = res.Title;
        this.BindingContext = CardLink;
        Rep = GenericRep;
        _service = service;
        IsUpdat = isUpdate;
        CardLinkRef.AccountLinkId = res.Id;
        CardLinkRef.CardId = cardId;
        LoadData();
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        MopupService.Instance.PopAsync();
    }

    void LoadData()
    {

        //LinkType.Add(new LinkTypeModel() { Id = 1, Name = "Url" });
        //LinkType.Add(new LinkTypeModel() { Id = 2, Name = "Email" });
        //LinkType.Add(new LinkTypeModel() { Id = 3, Name = "Phone" });
        //LinkType.Add(new LinkTypeModel() { Id = 4, Name = "Text" });

        LinkType.Add("Url");
        LinkType.Add("Email");
        LinkType.Add("Phone");
        LinkType.Add("Text");
        TypePicker.ItemsSource = LinkType;
        TypePicker.SelectedIndex = CardLink.CardLinkType - 1;

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

    private async void Save_Clicked(object sender, EventArgs e)
    {
        string valid = "";
        if (string.IsNullOrEmpty(ValueEn.Text))
        {
            var toast = Toast.Make($"{AppResources.msgFRValue}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
            return;
        }
        else
        {
            valid = CheckStringType(ValueEn.Text);
        }

        if (valid == "URL" && TypePicker.SelectedIndex == 0)
        {
            await SaveClick();
        }
        else if(valid == "Email" && TypePicker.SelectedIndex == 1)
        {
            await SaveClick();
        }
        else if (valid == "Phone Number" && TypePicker.SelectedIndex == 2)
        {
            await SaveClick();
        }
        else if (valid == "Text" && TypePicker.SelectedIndex == 3)
        {
            await SaveClick();
        }
        else
        {
            var toast = Toast.Make($"{AppResources.msgThevaluenotmatchCardLinkType}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }


    public string CheckStringType(string input)
    {
        // Email pattern
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        // URL pattern
        string urlPattern = @"^(http|https)://[^\s/$.?#].[^\s]*$";

        // Phone number pattern (international format: +123456789 or local 123-456-7890)
        string phonePattern = @"^(\+?\d{1,3})?[-.\s]?\(?\d{2,4}\)?[-.\s]?\d{3}[-.\s]?\d{3,4}$";
        // Any string pattern
        string pattern = @"^[\s\S]+$"; // Matches any plain text, including new lines

        if (Regex.IsMatch(input, emailPattern))
        {
            return "Email";
        }
        else if (Regex.IsMatch(input, urlPattern))
        {
            return "URL";
        }
        else if (Regex.IsMatch(input, phonePattern))
        {
            return "Phone Number";
        }
        else if (Regex.IsMatch(input, pattern))
        {
            return "Text";
        }
        else
        {
            return "Unknown";
        }
    }

    async Task SaveClick()
    {
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            this.IsEnabled = false;
            this.CloseWhenBackgroundIsClicked = false;  

            string UserToken = await _service.UserToken();
            var requestDto = new CardLinkRequest
            {
                AccountLinkId = CardLinkRef.AccountLinkId,
                CardLinkType = TypePicker.SelectedIndex + 1,
                ValueOf = ValueEn.Text,
            };
            string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
           
            if (IsUpdat == 0 )
            {
                var json = await Rep.PostTRAsync<CardLinkRequest, CardLinkResponse>($"{ApiConstants.CardLinkUpdateApi}{accid}/Card/{CardLinkRef.CardId}/CardLink/{CardLinkRef.Id}", requestDto, UserToken);

                if (json.Item1 != null)
                {
                    var toast = Toast.Make($"{AppResources.msgSuccessfullyUpdateLink}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                    await MopupService.Instance.PopAsync();
                    MessagingCenter.Send(this, "CreateLink", true);
                }
                else if (json.Item2 != null)
                {
                    var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }
            else if(IsUpdat == 1)
            {
                var json = await Rep.PostTRAsync<CardLinkRequest, CardLinkResponse>($"{ApiConstants.CardLinkUpdateApi}{accid}/Card/{CardLinkRef.CardId}/CardLink", requestDto, UserToken);

                if (json.Item1 != null)
                {
                    var toast = Toast.Make($"{AppResources.msgSuccessfullyAddLink}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                    await MopupService.Instance.PopAsync();
                    MessagingCenter.Send(this, "CreateLink", true);
                }
                else if (json.Item2 != null)
                {
                    var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }

            this.IsEnabled = true;
            this.CloseWhenBackgroundIsClicked = true;
        }
    }
}
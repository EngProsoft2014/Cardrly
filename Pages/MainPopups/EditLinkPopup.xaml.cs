using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.CardLink;
using CommunityToolkit.Maui.Alerts;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System.Text.RegularExpressions;

namespace Cardrly.Pages.MainPopups;

public partial class EditLinkPopup : Mopups.Pages.PopupPage
{
    CardLinkResponse CardLink = new CardLinkResponse();
    CardLinkResponse CardLinkRef = new CardLinkResponse();
    List<string> LinkType = new List<string>();
    int IsUpdat = 0; // 1 - Add & 0 - Update

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion
    public EditLinkPopup(CardLinkResponse cardLink, IGenericRepository GenericRep, Services.Data.ServicesService service)
	{
		InitializeComponent();
        Rep = GenericRep;
        _service = service;
        CardLink = new CardLinkResponse()
        {
            ValueOf = cardLink.ValueOf,
            CardLinkType = cardLink.CardLinkType,
        };
        CardLinkRef = cardLink;
        ValueEn.Text = CardLink.ValueOf;
        LoadData();
    }

    public EditLinkPopup(int isUpdate,string accountLinkId,string cardId, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        Rep = GenericRep;
        _service = service;
        IsUpdat = isUpdate;
        CardLinkRef.AccountLinkId = accountLinkId;
        CardLinkRef.CardId = cardId;
        hdrlbl.Text = "Add Link";
        LoadData();
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        MopupService.Instance.PopAsync();
    }

    void LoadData()
    {
        LinkType.Add("Url");
        LinkType.Add("Email");
        LinkType.Add("Phone");
        TypePicker.ItemsSource = LinkType;
        TypePicker.SelectedIndex = CardLink.CardLinkType - 1;
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        string valid = "";
        if (string.IsNullOrEmpty(ValueEn.Text))
        {
            var toast = Toast.Make("Require Field : Value", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
        else
        {
            var toast = Toast.Make($"The value not match Card Link Type", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
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
        else
        {
            return "Unknown";
        }
    }

    async Task SaveClick()
    {
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            string UserToken = await _service.UserToken();
            var requestDto = new CardLinkRequest
            {
                AccountLinkId = CardLinkRef.AccountLinkId,
                CardLinkType = TypePicker.SelectedIndex + 1,
                ValueOf = ValueEn.Text,
            };
            string accid = Preferences.Default.Get(ApiConstants.AccountId, "");
            UserDialogs.Instance.ShowLoading();
            if (IsUpdat == 0 )
            {
                var json = await Rep.PostTRAsync<CardLinkRequest, CardLinkResponse>($"{ApiConstants.CardLinkUpdateApi}{accid}/Card/{CardLinkRef.CardId}/CardLink/{CardLinkRef.Id}", requestDto, UserToken);
                UserDialogs.Instance.HideHud();
                if (json.Item1 != null)
                {
                    var toast = Toast.Make($"Successfully Add Link", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                    await MopupService.Instance.PopAsync();
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
                UserDialogs.Instance.HideHud();
                if (json.Item1 != null)
                {
                    var toast = Toast.Make($"Successfully Add Link", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                    await MopupService.Instance.PopAsync();
                }
                else if (json.Item2 != null)
                {
                    var toast = Toast.Make($"{json.Item2!.errors!.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }
            
            
        }
    }
}
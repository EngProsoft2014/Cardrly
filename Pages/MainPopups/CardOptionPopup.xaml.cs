using Controls.UserDialogs.Maui;
using Mopups.Services;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Mode_s.Card;
using Cardrly.ViewModels;
using System.Collections.ObjectModel;
using Plugin.NFC;
using System.Text;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System;
using CommunityToolkit.Maui.Alerts;

namespace Cardrly.Pages.MainPopups;

public partial class CardOptionPopup : Mopups.Pages.PopupPage
{
    CardResponse Card = new CardResponse();

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

        // Initialize NFC Plugin
        CrossNFC.Current.StartListening();
    } 
    #endregion

    #region Methods
    private async void TapGestureRecognizer_ShareCard(object sender, TappedEventArgs e)
    {
        await Share.RequestAsync(new ShareTextRequest
        {
            Uri = Card.CardUrl,
            Title = "Share Web Link"
        });
    }
    private async void TapGestureRecognizer_EditCaed(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        var vm = new AddCustomCardViewModel(Card, Rep, _service);
        var page = new AddCustomCardPage(vm);
        page.BindingContext = vm;
        await App.Current!.MainPage!.Navigation.PushAsync(page);
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }

    private async void TapGestureRecognizer_PreviewCard(object sender, TappedEventArgs e)
    {
        try
        {
            Uri uri = new Uri(Card.CardUrl!);
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
        bool ans = await DisplayAlert("Question", "Are you sure to delete This Card", "Ok", "Cancel");
        if (ans)
        {
            this.IsEnabled = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                await Rep.PostEAsync($"{ApiConstants.CardDeleteApi}{AccId}/Card/{Card.Id}/Delete", UserToken);
                UserDialogs.Instance.HideHud();
                await MopupService.Instance.PopAsync();
            }
            this.IsEnabled = true;
        }
    }

    private async void TapGestureRecognizer_Cancel(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    } 
    #endregion

    public const string ALERT_TITLE = "NFC";
    public const string MIME_TYPE = "application/com.companyname.nfcsample";

    NFCNdefTypeFormat _type;
    bool _makeReadOnly = false;
    bool _eventsAlreadySubscribed = false;
    bool _isDeviceiOS = false;

    /// <summary>
    /// Property that tracks whether the Android device is still listening,
    /// so it can indicate that to the user.
    /// </summary>
    public bool DeviceIsListening
    {
        get => _deviceIsListening;
        set
        {
            _deviceIsListening = value;
            OnPropertyChanged(nameof(DeviceIsListening));
        }
    }
    private bool _deviceIsListening;

    private bool _nfcIsEnabled;
    public bool NfcIsEnabled
    {
        get => _nfcIsEnabled;
        set
        {
            _nfcIsEnabled = value;
            OnPropertyChanged(nameof(NfcIsEnabled));
            OnPropertyChanged(nameof(NfcIsDisabled));
        }
    }

    public bool NfcIsDisabled => !NfcIsEnabled;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // In order to support Mifare Classic 1K tags (read/write), you must set legacy mode to true.
        CrossNFC.Legacy = false;

        if (CrossNFC.IsSupported)
        {
            if (!CrossNFC.Current.IsAvailable)
                await ShowAlert("NFC is not available");

            NfcIsEnabled = CrossNFC.Current.IsEnabled;
            if (!NfcIsEnabled)
                await ShowAlert("NFC is disabled");

            if (DeviceInfo.Platform == DevicePlatform.iOS)
                _isDeviceiOS = true;

            CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;

            await AutoStartAsync().ConfigureAwait(false);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Task.Run(() => StopListening());
        return base.OnBackButtonPressed();
    }

    /// <summary>
    /// Auto Start Listening
    /// </summary>
    /// <returns></returns>
    async Task AutoStartAsync()
    {
        // Some delay to prevent Java.Lang.IllegalStateException "Foreground dispatch can only be enabled when your activity is resumed" on Android
        await Task.Delay(500);
        await StartListeningIfNotiOS();
    }

    /// <summary>
    /// Subscribe to the NFC events
    /// </summary>
    void SubscribeEvents()
    {
        if (_eventsAlreadySubscribed)
            UnsubscribeEvents();

        _eventsAlreadySubscribed = true;

        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
        CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
        CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
        CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
        CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;

        if (_isDeviceiOS)
            CrossNFC.Current.OniOSReadingSessionCancelled += Current_OniOSReadingSessionCancelled;
    }

    /// <summary>
    /// Unsubscribe from the NFC events
    /// </summary>
    void UnsubscribeEvents()
    {
        CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
        CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
        CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
        CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;
        CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;

        if (_isDeviceiOS)
            CrossNFC.Current.OniOSReadingSessionCancelled -= Current_OniOSReadingSessionCancelled;

        _eventsAlreadySubscribed = false;
    }

    /// <summary>
    /// Event raised when Listener Status has changed
    /// </summary>
    /// <param name="isListening"></param>
    void Current_OnTagListeningStatusChanged(bool isListening) => DeviceIsListening = isListening;

    /// <summary>
    /// Event raised when NFC Status has changed
    /// </summary>
    /// <param name="isEnabled">NFC status</param>
    async void Current_OnNfcStatusChanged(bool isEnabled)
    {
        NfcIsEnabled = isEnabled;
        await ShowAlert($"NFC has been {(isEnabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Event raised when a NDEF message is received
    /// </summary>
    /// <param name="tagInfo">Received <see cref="ITagInfo"/></param>
    async void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        if (tagInfo == null)
        {
            await ShowAlert("No tag found");
            return;
        }

        // Customized serial number
        var identifier = tagInfo.Identifier;
        var serialNumber = NFCUtils.ByteArrayToHexString(identifier, ":");
        var title = !string.IsNullOrWhiteSpace(serialNumber) ? $"Tag [{serialNumber}]" : "Tag Info";

        if (!tagInfo.IsSupported)
        {
            await ShowAlert("Unsupported tag (app)", title);
        }
        else if (tagInfo.IsEmpty)
        {
            await ShowAlert("Empty tag", title);
        }
        else
        {
            var first = tagInfo.Records[0];
            await ShowAlert(GetMessage(first), title);
        }
    }

    /// <summary>
    /// Event raised when user cancelled NFC session on iOS 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Current_OniOSReadingSessionCancelled(object sender, EventArgs e) => Debug("iOS NFC Session has been cancelled");

    /// <summary>
    /// Event raised when data has been published on the tag
    /// </summary>
    /// <param name="tagInfo">Published <see cref="ITagInfo"/></param>
    async void Current_OnMessagePublished(ITagInfo tagInfo)
    {
        try
        {
            CrossNFC.Current.StopPublishing();
            if (tagInfo.IsEmpty)
                await ShowAlert("Formatting tag operation successful");
            else
                await ShowAlert("Writing tag operation successful");
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    /// <summary>
    /// Event raised when a NFC Tag is discovered
    /// </summary>
    /// <param name="tagInfo"><see cref="ITagInfo"/> to be published</param>
    /// <param name="format">Format the tag</param>
    async void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {

        if (!CrossNFC.Current.IsWritingTagSupported)
        {
            await ShowAlert("Writing tag is not supported on this device");
            return;
        }

        string vCardData = "BEGIN:VCARD\n" +
              "VERSION:3.0\n" +
              $"FN:{Card.PersonName + " " + Card.PersonNikeName}\n" +
              "ORG:Engprosoft company\n" +
              "TEL:+18324686031\n" +
              "EMAIL:engprosoftcompany@gmail.com\n" +
              $"ADR:{Card.location}\n" +
              $"URL:{Card.CardUrl}\n" +
              //$"LOGO:{Card.UrlImgProfile}\n" +
              "END:VCARD";


        //string vCardData = "BEGIN:VCARD\n" +
        //                  "VERSION:3.0\n" +
        //                  "FN:Tarek Gaber\n" +
        //                  "ORG:Engprosoft company\n" +
        //                  "TEL:+18324686031\n" +
        //                  "EMAIL:engprosoftcompany@gmail.com\n" +
        //                  "END:VCARD";

        //string vCardData = Card.ToString();


        try
        {
            NFCNdefRecord record = null;
            //if (_type == NFCNdefTypeFormat.Mime)
            //{
            //    record = new NFCNdefRecord
            //    {
            //        TypeFormat = NFCNdefTypeFormat.Mime,
            //        MimeType = "text/vcard",
            //        Payload = NFCUtils.EncodeToByteArray(vCardData),
            //        Uri = Card.CardUrl
            //    };
            //}

            if (_type == NFCNdefTypeFormat.Uri)
            {
                record = new NFCNdefRecord
                {
                    TypeFormat = NFCNdefTypeFormat.Uri,
                    Payload = NFCUtils.EncodeToByteArray(Card.CardUrl)
                };
            }

            if (!format && record == null)
                throw new Exception("Record can't be null.");

            tagInfo.Records = new[] { record };

            if (format)
                CrossNFC.Current.ClearMessage(tagInfo);
            else
            {
                CrossNFC.Current.PublishMessage(tagInfo, _makeReadOnly);
            }
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    /// <summary>
    /// Start listening for NFC Tags when "READ TAG" button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StartListening(object sender, System.EventArgs e) => await BeginListening();

    /// <summary>
    /// Stop listening for NFC tags
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StopListening(object sender, System.EventArgs e) => await StopListening();

    /// <summary>
    /// Start publish operation to write the tag (TEXT) when <see cref="Current_OnTagDiscovered(ITagInfo, bool)"/> event will be raised
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StartWriting(object sender, System.EventArgs e) => await Publish(NFCNdefTypeFormat.WellKnown);

    /// <summary>
    /// Start publish operation to write the tag (URI) when <see cref="Current_OnTagDiscovered(ITagInfo, bool)"/> event will be raised
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StartWriting_Uri(object sender, System.EventArgs e) => await Publish(NFCNdefTypeFormat.Uri);


    /// <summary>
    /// Start publish operation to write the tag (CUSTOM) when <see cref="Current_OnTagDiscovered(ITagInfo, bool)"/> event will be raised
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StartWriting_Custom(object sender, System.EventArgs e) => await Publish(NFCNdefTypeFormat.Mime);


    /// <summary>
    /// Start publish operation to format the tag when <see cref="Current_OnTagDiscovered(ITagInfo, bool)"/> event will be raised
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_FormatTag(object sender, System.EventArgs e) => await Publish();

    /// <summary>
    /// Task to publish data to the tag
    /// </summary>
    /// <param name="type"><see cref="NFCNdefTypeFormat"/></param>
    /// <returns>The task to be performed</returns>
    async Task Publish(NFCNdefTypeFormat? type = null)
    {
        await StartListeningIfNotiOS();
        try
        {
            _type = NFCNdefTypeFormat.Empty;

            if (type.HasValue) _type = type.Value;
            CrossNFC.Current.StartPublishing(!type.HasValue);
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    /// <summary>
    /// Returns the tag information from NDEF record
    /// </summary>
    /// <param name="record"><see cref="NFCNdefRecord"/></param>
    /// <returns>The tag information</returns>
    string GetMessage(NFCNdefRecord record)
    {
        var message = $"Message: {record.Message}";
        message += Environment.NewLine;
        message += $"RawMessage: {Encoding.UTF8.GetString(record.Payload)}";
        message += Environment.NewLine;
        message += $"Type: {record.TypeFormat}";

        if (!string.IsNullOrWhiteSpace(record.MimeType))
        {
            message += Environment.NewLine;
            message += $"MimeType: {record.MimeType}";
        }

        return message;
    }

    /// <summary>
    /// Write a debug message in the debug console
    /// </summary>
    /// <param name="message">The message to be displayed</param>
    void Debug(string message) => System.Diagnostics.Debug.WriteLine(message);

    /// <summary>
    /// Display an alert
    /// </summary>
    /// <param name="message">Message to be displayed</param>
    /// <param name="title">Alert title</param>
    /// <returns>The task to be performed</returns>
    Task ShowAlert(string message, string title = null) => DisplayAlert(string.IsNullOrWhiteSpace(title) ? ALERT_TITLE : title, message, "OK");

    /// <summary>
    /// Task to start listening for NFC tags if the user's device platform is not iOS
    /// </summary>
    /// <returns>The task to be performed</returns>
    async Task StartListeningIfNotiOS()
    {
        if (_isDeviceiOS)
        {
            SubscribeEvents();
            return;
        }
        await BeginListening();
    }

    /// <summary>
    /// Task to safely start listening for NFC Tags
    /// </summary>
    /// <returns>The task to be performed</returns>
    async Task BeginListening()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SubscribeEvents();
                CrossNFC.Current.StartListening();
            });
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    /// <summary>
    /// Task to safely stop listening for NFC tags
    /// </summary>
    /// <returns>The task to be performed</returns>
    async Task StopListening()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CrossNFC.Current.StopListening();
                UnsubscribeEvents();
            });
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    
}
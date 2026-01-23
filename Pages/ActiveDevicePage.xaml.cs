using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Models.Card;
using Cardrly.Models;
using Cardrly.Models.Devices;
using Cardrly.Pages.MainPopups;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Azure;
using Mopups.Services;
using Plugin.NFC;
using System.Text;
using ZXing.Net.Maui.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace Cardrly.Pages;

public partial class ActiveDevicePage : Controls.CustomControl
{
    ActiveDeviceViewModel Model;

    string SetupUri = "";
    int deviceType;
    string deviceId;
    int isInserted = 0;
    public ActiveDevicePage(ActiveDeviceViewModel model)
    {
        InitializeComponent();
        this.BindingContext = model;
        Model = model;
    }

    #region Nfc Setup
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
            {
                if (isInserted == 0)
                {
                    await Model.DeviceClick(SetupUri, deviceType, deviceId);
                    isInserted = 1;
                }

                await ShowAlert("Writing tag operation successful");
            }
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
        deviceId = tagInfo.SerialNumber;
        isInserted = 0;
        if (!CrossNFC.Current.IsWritingTagSupported)
        {
            await ShowAlert("Writing tag is not supported on this device");
            return;
        }

        string vCardData = "BEGIN:VCARD\n" +
              "VERSION:3.0\n" +
              $"FN:{Model.DetailsResponse.PersonName + " " + Model.DetailsResponse.PersonNikeName}\n" +
              "ORG:Engprosoft company\n" +
              "TEL:+18324686031\n" +
              "EMAIL:engprosoftcompany@gmail.com\n" +
              $"ADR:{Model.DetailsResponse.location}\n" +
              $"URL:{Model.DetailsResponse.CardUrlVM}\n" +
              "END:VCARD";

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
                    Payload = NFCUtils.EncodeToByteArray(SetupUri)
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
    async void Button_Clicked_StartWriting_Uri(object sender, TappedEventArgs e)
    {
        if (StaticMember.CheckPermission(ApiConstants.AddDevices))
        {
            var Item = e.Parameter as DevicesTypeModel;

            deviceType = Item!.DeviceNumber;

            if (Item!.DeviceName == "QR")
            {                   
                MessagingCenter.Subscribe<ScanQrPage, ReturnQrCodeModel>(this, "QRCodeValue", async (sender, model) =>
                {

                    if (Guid.TryParse(model.matchValue, out Guid parseGuid))
                    {
                        if (model.isManualQrCode == false) 
                        {
                            var existingPopup = MopupService.Instance.PopupStack.FirstOrDefault(p => p is InsertDevicePopup);

                            if (existingPopup == null)
                            {
                                var page2 = new InsertDevicePopup(false);
                                //Get Link  
                                page2.DeviceClose += async (Uri, UriRedirect) =>
                                {
                                    SetupUri = UriRedirect;
                                    await Model.DeviceClick(SetupUri, deviceType, model.matchValue);
                                };
                                await MopupService.Instance.PushAsync(page2, true);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(model.scanUriRedirectValue))
                            {
                                await Model.DeviceClick(model.scanUriRedirectValue, deviceType, model.matchValue);
                            }
                        }
                    }
                });

                await App.Current!.MainPage!.Navigation.PushAsync(new ScanQrPage());
            }
            else if (Item!.DeviceName == "Stand" || Item!.DeviceName == "CustomNFC")
            {
                var page = new InsertDevicePopup(false);
                page.DeviceClose += async (Uri , UriRedirect) =>
                {
                    SetupUri = UriRedirect;

#if ANDROID
                var toast = Toast.Make($"Near the stand now.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
#endif
                    await Publish(NFCNdefTypeFormat.Uri);
                };
                await MopupService.Instance.PushAsync(page, true);
            }
            else
            {
                SetupUri = Model.DetailsResponse.CardUrlVM!;

#if ANDROID
                var toast = Toast.Make($"Near the Device now.", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
#endif
                await Publish(NFCNdefTypeFormat.Uri);
            }
        }
        else
        {
            var toast = Toast.Make($"{AppResources.msgPermissionToDoAction}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }
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

    #endregion

    private void ClearCard_Tapped(object sender, TappedEventArgs e)
    {
        CrossNFC.Current.StartPublishing(clearMessage: true);
    }
}
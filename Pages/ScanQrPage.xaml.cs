
using System.Text.RegularExpressions;
using System;
using ZXing.Net.Maui;
using Cardrly.Resources.Lan;
using Mopups.Services;
using Cardrly.Pages.MainPopups;
using Cardrly.Models;
using CommunityToolkit.Maui.Alerts;

namespace Cardrly.Pages;

public partial class ScanQrPage : Controls.CustomControl
{
    public delegate void QrDelegte(string QrValue);
    public event QrDelegte QrClose;
    private bool isScanning = false; // Flag to prevent multiple detections

    public ScanQrPage()
    {
        InitializeComponent();
        Init();
    }

    void Init()
    {
        cameraBarcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            TryInverted = true,
            TryHarder = true,
            AutoRotate = true,
            Multiple = false
        };
    }

    private async void cameraBarcodeReaderView_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        if (isScanning || e.Results == null || e.Results.Count() == 0)
            return;

        isScanning = true; // Prevent further detections

        // Extract the first barcode value
        string scannedValue = e.Results.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(scannedValue))
        {
            isScanning = false; // Reset flag if no valid value
            return;
        }

        await CheckQrCode(scannedValue, string.Empty, false);
    }


    async Task CheckQrCode(string scanValue, string scanValueRedirect, bool isMnlQrCode)
    {
        // Define the regex pattern for a GUID
        string pattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        Match match = Regex.Match(scanValue, pattern);

        if (Guid.TryParse(match.Value, out Guid parsedGuid))
        {
            ReturnQrCodeModel oReturnQrCodeModel = new ReturnQrCodeModel();

            if (isMnlQrCode && !string.IsNullOrEmpty(scanValueRedirect))
            {
                oReturnQrCodeModel.scanUriRedirectValue = scanValueRedirect;
            }

            // Stop scanning to prevent multiple detections
            MainThread.BeginInvokeOnMainThread(() =>
            {
                cameraBarcodeReaderView.IsDetecting = false; // Stops scanning
            });

            oReturnQrCodeModel.matchValue = match.Value;
            oReturnQrCodeModel.scanUriValue = scanValue;
            oReturnQrCodeModel.isManualQrCode = isMnlQrCode;

            MessagingCenter.Send(this, "QRCodeValue", oReturnQrCodeModel);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await App.Current!.MainPage!.Navigation.PopAsync();
                await Task.Delay(1000); // Small delay for UI updates
                isScanning = false; // Reset the flag
                cameraBarcodeReaderView.IsDetecting = true; // Restart scanning if needed
            });
        }
        else
        {
            isScanning = false; // Reset flag if invalid QR code
            var toast = Toast.Make($"{AppResources.msgInvalidQRcode}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            await toast.Show();
        }
    }
    //private async void cameraBarcodeReaderView_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    //{
    //    // Define the regex pattern for a GUID
    //    string pattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
    //    // Extract the GUID
    //    Match match = Regex.Match(e.Results.FirstOrDefault()!.Value, pattern);
    //    QrClose?.Invoke(match.Value);
    //}

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        App.Current!.MainPage!.Navigation.PopAsync();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var page = new InsertDevicePopup(true);
        page.DeviceClose += async (Uri, UriRedirect) =>
        {
            await CheckQrCode(Uri, UriRedirect, true);
        };
        await MopupService.Instance.PushAsync(page, true);
    }
}

using System.Text.RegularExpressions;
using System;

namespace Cardrly.Pages;

public partial class ScanQrPage : Controls.CustomControl
{
    public delegate void QrDelegte(string QrValue);
    public event QrDelegte QrClose;
    public ScanQrPage()
	{
		InitializeComponent();
	}

    private async void cameraBarcodeReaderView_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        // Define the regex pattern for a GUID
        string pattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        // Extract the GUID
        Match match = Regex.Match(e.Results.FirstOrDefault()!.Value, pattern);
        QrClose?.Invoke(match.Value);
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        App.Current!.MainPage!.Navigation.PopAsync();
    }
}
using Cardrly.Controls;
using Cardrly.Resources.Lan;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using Mopups.Services;
using SkiaSharp;
using System.Globalization;
using System.IO;


namespace Cardrly.Pages.MainPopups;

public partial class AddAttachmentsPopup : Mopups.Pages.PopupPage
{
    public delegate void imageDelegte(string img,string imagePath);
    public event imageDelegte ImageClose;
    byte[] Image;
    public AddAttachmentsPopup(bool IsScan = false)
	{
		InitializeComponent();
        //EditImage.IsVisible = !IsScan;
        //EditImageBox.IsVisible = !IsScan;
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

    public AddAttachmentsPopup(bool IsScan, byte[] bytes)
    {
        InitializeComponent();
        Image = bytes;
        //EditImage.IsVisible = !IsScan;
        //EditImageBox.IsVisible = !IsScan;
    }
    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void TapGestureRecognizer_Tapped_Cam(object sender, TappedEventArgs e)
    {
        try
        {
            // Check if the camera is available
            if (MediaPicker.Default.IsCaptureSupported)
            {
                // Capture the photo
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    using var stream = await photo.OpenReadAsync();
                    using var memoryStream = new MemoryStream();

                    // Load the image into SkiaSharp and resize it
                    using var originalBitmap = SKBitmap.Decode(stream);
                    var resizedBitmap = originalBitmap.Resize(new SKImageInfo(800, 600), SKFilterQuality.Medium);

                    using var image = SKImage.FromBitmap(resizedBitmap);
                    using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75); // Compression level: 75%
                    data.SaveTo(memoryStream);

                    // Display the image
                    ImageClose.Invoke(Convert.ToBase64String(memoryStream.ToArray()), photo.FullPath);
                }
            }
            else
            {
                await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgCameranotsupported}", $"{AppResources.msgOk}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert($"{AppResources.msgWarning}", ex.Message, $"{AppResources.msgOk}");
        }
    }

    private async void TapGestureRecognizer_Tapped_Pic(object sender, TappedEventArgs e)
    {
        try
        {

            // Open the photo gallery
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo != null)
            {
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();

                // Load the image into SkiaSharp and resize it
                using var originalBitmap = SKBitmap.Decode(stream);
                var resizedBitmap = originalBitmap.Resize(new SKImageInfo(800, 600), SKFilterQuality.Medium);

                using var image = SKImage.FromBitmap(resizedBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75); // Compression level: 75%
                data.SaveTo(memoryStream);

                // Display the selected photo in the Image control
                ImageClose.Invoke(Convert.ToBase64String(memoryStream.ToArray()), photo.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert($"{AppResources.msgWarning}", ex.Message, $"{AppResources.msgOk}");
        }
        
    }

    //private async void TapGestureRecognizer_Tapped_EditImage(object sender, TappedEventArgs e)
    //{
    //    this.IsEnabled = false;
    //    if (Image != null)
    //    {
    //        var page = new ImageEditorPage(Image);
    //        page.ImageEditClose += (img,Loc) =>
    //        {
    //            if (!string.IsNullOrEmpty(img) & !string.IsNullOrEmpty(Loc))
    //            {
    //                ImageClose.Invoke(img,Loc);
    //            }
    //        };
    //        await App.Current!.MainPage!.Navigation.PushAsync(page);
    //    }
    //    else
    //    {
    //        var toast = Toast.Make($"{AppResources.msgPleaseSelectImageFirst}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
    //        await toast.Show();
    //    }
    //    //this.IsEnabled = true;
    //    await MopupService.Instance.PopAsync();
    //}
}
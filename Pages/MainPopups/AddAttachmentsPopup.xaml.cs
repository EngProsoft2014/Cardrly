using CommunityToolkit.Maui.Views;
using Mopups.Services;
using SkiaSharp;


namespace Cardrly.Pages.MainPopups;

public partial class AddAttachmentsPopup : Mopups.Pages.PopupPage
{
    public delegate void imageDelegte(string img,string imagePath);
    public event imageDelegte ImageClose;

    public AddAttachmentsPopup()
	{
		InitializeComponent();
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
                await DisplayAlert("error", "Camera not supported", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("error", ex.Message, "OK");
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
            await DisplayAlert("TripBliss.Resources.Language.AppResources.error", ex.Message, "TripBliss.Resources.Language.AppResources.OK");
        }
    }

    private async void TapGestureRecognizer_Tapped_Pic_pdf(object sender, TappedEventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a PDF file",
                FileTypes = FilePickerFileType.Pdf
            });

            if (result != null && result.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                // Get the file path to save it
                var stream = await result.OpenReadAsync();

                // Display the image
                ImageClose.Invoke(Convert.ToBase64String(Helpers.Utility.ReadToEnd(stream)), result.FullPath);
            }
        }
        catch (Exception ex)
        {
            await App.Current!.MainPage!.DisplayAlert("TripBliss.Resources.Language.AppResources.error", $"{ex.Message}", "TripBliss.Resources.Language.AppResources.OK");
        }
    }
}
using System.IO;

namespace Cardrly.Pages;

public partial class ImageEditorPage : ContentPage
{
	public delegate void imageDelegte(string img,string Loc);
    public event imageDelegte ImageEditClose;
	public ImageEditorPage(byte[] bytes)
	{
		InitializeComponent();
		ImgEditor.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
	}


    private async void ImgEditor_ImageSaving(object sender, Syncfusion.Maui.ImageEditor.ImageSavingEventArgs e)
    {
        if (e.ImageStream != null)
        {
            byte[] byteArray;
            using (var memoryStream = new MemoryStream())
            {
                #if ANDROID || IOS || MACCATALYST
                        e.CompressionQuality = 0.5F;
                #endif
                e.ImageStream.CopyTo(memoryStream);
                byteArray = memoryStream.ToArray();
            }
            ImageEditClose.Invoke(Convert.ToBase64String(byteArray),e.FilePath);
            await App.Current!.MainPage!.Navigation.PopAsync();
        }
    }
}
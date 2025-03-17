
namespace Cardrly.Pages;

public partial class FullScreenImage : Controls.CustomControl
{
	public FullScreenImage()
	{
		InitializeComponent();
	}

    public FullScreenImage(ImageSource sourceImage)
    {
        InitializeComponent();
        imgFullScreen.Source = sourceImage;
    }

    public FullScreenImage(string URLImage)
    {
        InitializeComponent();
        imgFullScreen.Source = URLImage;
    }

    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
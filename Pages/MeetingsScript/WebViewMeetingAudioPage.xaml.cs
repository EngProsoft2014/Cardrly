using Microsoft.Maui.Controls.Compatibility;
using System.Threading.Tasks;

namespace Cardrly.Pages.MeetingsScript;

public partial class WebViewMeetingAudioPage : Controls.CustomControl
{
	public WebViewMeetingAudioPage(string url)
	{
		InitializeComponent();
        audioWebView.Source = new Uri(url);

    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (audioWebView != null)
        {
            try
            {
                // Stop <audio> or <video> elements if they exist
                await audioWebView.EvaluateJavaScriptAsync(@"
                var audios = document.getElementsByTagName('audio');
                for (var i = 0; i < audios.length; i++) { audios[i].pause(); audios[i].src=''; }

                var videos = document.getElementsByTagName('video');
                for (var i = 0; i < videos.length; i++) { videos[i].pause(); videos[i].src=''; }
            ");
            }
            catch
            {

            }
#if IOS
        try
        {
            // Remove from UI to release underlying WKWebView
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var parent = audioWebView.Parent as Layout<View>;
                parent?.Children.Remove(audioWebView);
                audioWebView.Handler?.DisconnectHandler();
                audioWebView = null;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebView dispose error: {ex.Message}");
        }
#else
            // Android fallback
            audioWebView.Source = new HtmlWebViewSource { Html = "<html><body></body></html>" };
            audioWebView.Handler?.DisconnectHandler();
#endif
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
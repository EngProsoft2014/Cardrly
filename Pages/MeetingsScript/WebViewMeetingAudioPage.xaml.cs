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
                // Try to stop audio/video via JS first
                await audioWebView.EvaluateJavaScriptAsync(@"
                var audios = document.getElementsByTagName('audio');
                for (var i = 0; i < audios.length; i++) { 
                    audios[i].pause(); 
                    audios[i].src=''; 
                }

                var videos = document.getElementsByTagName('video');
                for (var i = 0; i < videos.length; i++) { 
                    videos[i].pause(); 
                    videos[i].src=''; 
                }
            ");
            }
            catch
            {
                // ignore
            }

#if IOS
        // Hard stop and dispose for iOS
        if (audioWebView.Handler?.PlatformView is WebKit.WKWebView wk)
        {
            wk.StopLoading();
            wk.LoadHtmlString("<html><body></body></html>", null);
            wk.RemoveFromSuperview();
            wk.Dispose();
        }
#endif
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
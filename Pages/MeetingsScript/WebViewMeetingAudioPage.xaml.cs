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
            await StopWebViewMediaAsync(audioWebView);
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async Task StopWebViewMediaAsync(WebView webView)
    {
        if (webView == null)
            return;

        try
        {
#if IOS
        // Get native WKWebView
        if (webView.Handler?.PlatformView is WebKit.WKWebView wkWebView)
        {
            try
            {
                // 1. Stop <audio>/<video> via JS
                await webView.EvaluateJavaScriptAsync(@"
                    document.querySelectorAll('audio,video').forEach(m => {
                        m.pause();
                        m.removeAttribute('src');
                        m.load();
                    });
                ");
            }
            catch
            {
                // Ignore if JS evaluation fails (likely disposed)
            }

            // 2. Force stop any loading or playback
            wkWebView.StopLoading();

            // 3. Clear HTML and dispose safely
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    wkWebView.LoadHtmlString("<html><body></body></html>", null);
                    wkWebView.RemoveFromSuperview();
                    wkWebView.Dispose();
                }
                catch { /* swallow if already disposed */ }
            });
        }
#elif ANDROID
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

                // Android cleanup
                webView.Source = new HtmlWebViewSource { Html = "<html><body></body></html>" };
                webView.Handler?.DisconnectHandler();
            }
            catch
            {
                // ignore
            }

#endif
        }
        catch
        {
            // Never throw from teardown
        }
    }

}
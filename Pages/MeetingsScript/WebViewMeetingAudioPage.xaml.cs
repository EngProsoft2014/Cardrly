#if IOS
using CoreFoundation;
#endif
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
    
    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await StopWebViewMediaAsync(audioWebView);
    }

    private async Task StopWebViewMediaAsync(WebView webView)
    {
        if (webView == null)
            return;

        try
        {
#if IOS
        if (webView.Handler?.PlatformView is WebKit.WKWebView wkWebView)
        {
            try
            {
                // Stop any <audio>/<video> playback via JS
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
                // ignore if JS fails
            }

            // Safely clear the page — DO NOT dispose yet!
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    wkWebView.StopLoading();
                    wkWebView.LoadHtmlString("<html><body></body></html>", null);
                }
                catch
                {
                    // ignore
                }
            });
        }
#elif ANDROID
            try
            {
                await webView.EvaluateJavaScriptAsync(@"
                document.querySelectorAll('audio,video').forEach(m => {
                    m.pause();
                    m.removeAttribute('src');
                    m.load();
                });
            ");
            }
            catch { }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    webView.Source = new HtmlWebViewSource { Html = "<html><body></body></html>" };
                }
                catch { }
            });
#endif
        }
        catch
        {
            // Never throw from cleanup
        }
    }


}
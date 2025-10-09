using CoreFoundation;
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
        // Always run cleanup on the main thread for iOS UI stability
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                if (webView.Handler?.PlatformView is WebKit.WKWebView wkWebView)
                {
                    // Stop any network or media activity
                    wkWebView.StopLoading();

                    // Clear the HTML (breaks the audio context)
                    wkWebView.LoadHtmlString("<html><body></body></html>", null);

                    // Force release of the player after a small delay
                    DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, 300_000_000), () =>
                    {
                        try
                        {
                            wkWebView.RemoveFromSuperview();
                            wkWebView.Dispose();
                        }
                        catch
                        {
                            // ignore if already disposed
                        }
                    });
                }
            }
            catch
            {
                // ignore
            }
        });

#elif ANDROID
            try
            {
                // Safe to use JS on Android
                await webView.EvaluateJavaScriptAsync(@"
                document.querySelectorAll('audio,video').forEach(m => {
                    m.pause();
                    m.src='';
                });
            ");

                webView.Source = new HtmlWebViewSource { Html = "<html><body></body></html>" };

                // Delay a bit before disposing (to avoid ObjectDisposedException)
                await Task.Delay(100);
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
            // Final catch — never throw from cleanup
        }
    }


}
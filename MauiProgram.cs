using Cardrly.Helpers;
using Cardrly.Services.Data;
using CommunityToolkit.Maui;
using Controls.UserDialogs.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;
using ZXing.Net.Maui.Controls;

namespace Cardrly
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseUserDialogs()
                .ConfigureMopups()
                .ConfigureSyncfusionCore()
                .UseSkiaSharp()
                // Add this section anywhere on the builder:
                .UseSentry(options => {
                    // The DSN is the only required setting.
                    options.Dsn = "https://85844c8c0d12cfa4ab5ecf99e1986b75@o4508536170676224.ingest.us.sentry.io/4508536181096448";
                    // Add this to the SDK initialization callback
                    options.AddExceptionFilterForType<OperationCanceledException>();
                    // Use debug mode if you want to see what the SDK is doing.
                    // Debug messages are written to stdout with Console.Writeline,
                    // and are viewable in your IDE's debug console or with 'adb logcat', etc.
                    // This option is not recommended when deploying your application.
                    options.Debug = true;

                    // Set TracesSampleRate to 1.0 to capture 100% of transactions for tracing.
                    // We recommend adjusting this value in production.
                    options.TracesSampleRate = 1.0;

                    // Other Sentry options can be set here.
                })
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 6 Brands-Regular-400.otf", "FontIconBrand");
                    fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FontIcon");
                    fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FontIconSolid");
                    fonts.AddFont("ElMessiri-Regular.ttf", "Almarai-Regular");
                    fonts.AddFont("ElMessiri-Bold.ttf", "Almarai-Bold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<ServicesService>();
            builder.Services.AddScoped<IGenericRepository, GenericRepository>();
            // Add this to the SDK initialization callback
            
            return builder.Build();
        }

    }

}
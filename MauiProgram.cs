
using Cardrly.Helpers;
using Cardrly.Services;
using Cardrly.Services.Data;
using CommunityToolkit.Maui;
using Controls.UserDialogs.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Mopups.Hosting;
using Plugin.Maui.Audio;
//using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;
using ZXing.Net.Maui.Controls;
//using Plugin.Firebase.CloudMessaging;
//#if IOS
//using Plugin.Firebase.Core.Platforms.iOS;
//#elif ANDROID
//using Plugin.Firebase.Core.Platforms.Android;
//#endif

namespace Cardrly
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSentry(options =>
                {
                    // The DSN is the only required setting.
                    options.Dsn = "https://462387a2a643f8c993897ae3030c6438@o4508536170676224.ingest.us.sentry.io/4508890903216128";

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
                //.RegisterFirebaseServices()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .UseUserDialogs()
                .ConfigureMopups()
                .ConfigureSyncfusionCore()
                .UseBarcodeReader()
                .UseMauiMaps()
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
            builder.Services.AddDependencies();

            DependencyInjection.ControlsBackground();

#if ANDROID
            builder.Services.AddSingleton<ISaveContact, SaveContactAndroid>(); // No need for "Platforms"
#elif IOS
            builder.Services.AddSingleton<ISaveContact, SaveContactiOS>(); // No need for "Platforms"
#endif
            return builder.Build();
        }

//        private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
//        {
//            builder.ConfigureLifecycleEvents(events => {
//#if IOS
//        events.AddiOS(iOS => iOS.WillFinishLaunching((_, __) => {
//            CrossFirebase.Initialize();
//            FirebaseCloudMessagingImplementation.Initialize();
//            return false;
//        }));
//#elif ANDROID
//                events.AddAndroid(android => android.OnCreate((activity, _) =>
//                CrossFirebase.Initialize(activity)));
//#endif
//            });

//            return builder;
//        }
    }

}
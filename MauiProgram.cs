using Cardrly.Helpers;
using Cardrly.Services;
using Cardrly.Services.Data;
using CommunityToolkit.Maui;
using Controls.UserDialogs.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Plugin.Maui.Audio;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;
using ZXing.Net.Maui.Controls;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.CloudMessaging;



#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif
namespace Cardrly
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .RegisterFirebaseServices()
                // Add this section anywhere on the builder:
                .UseMauiCommunityToolkit()
                .UseUserDialogs()
                .ConfigureMopups()
                .ConfigureSyncfusionCore()
                .UseSkiaSharp()
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
            builder.Services.AddDependencies();

            DependencyInjection.ControlsBackground();

#if ANDROID
            builder.Services.AddSingleton<ISaveContact, SaveContactAndroid>(); // No need for "Platforms"
#elif IOS
            builder.Services.AddSingleton<ISaveContact, SaveContactiOS>(); // No need for "Platforms"
#endif
            return builder.Build();
        }

        private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        {
            builder.ConfigureLifecycleEvents(events => {
#if IOS
        events.AddiOS(iOS => iOS.WillFinishLaunching((_, __) => {
            CrossFirebase.Initialize();
            FirebaseCloudMessagingImplementation.Initialize();
            return false;
        }));
#elif ANDROID
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity)));
#endif
            });

            return builder;
        }
    }

}
using Cardrly.Helpers;
using Cardrly.Pages;
using Cardrly.Pages.Links;
using Cardrly.Pages.MainPopups;
using Cardrly.Services;
using Cardrly.Services.Data;
using Cardrly.ViewModels;
using Cardrly.ViewModels.Leads;
using Cardrly.ViewModels.Links;
using Microsoft.Maui.Handlers;
using Plugin.Maui.Audio;
using Cardrly.Services.AudioStream;

#if IOS
using Cardrly.Services.NativeAudioRecorder;
#endif




#if ANDROID
using Cardrly.Platforms.Android;
#elif IOS
using Cardrly.Platforms.iOS;
#endif
namespace Cardrly
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection Services)
        {
            #region ServiceServices
            Services.AddSingleton<ServicesService>();
            
            #endregion

            #region GenericRepository
            Services.AddScoped<IGenericRepository, GenericRepository>();
            #endregion

            #region ViewModels

            #region Lead ViewModels
            Services.AddTransient<LeadViewModel>();
            Services.AddTransient<AddLeadViewModel>();
            #endregion

            #region Links ViewModels
            Services.AddTransient<LinksViewModel>();
            Services.AddTransient<AddLinkViewModel>();
            #endregion

            #region Share ViewModels
            //Main DistributorsViewModels ViewModels
            Services.AddTransient<ActiveDeviceViewModel>();
            Services.AddTransient<AddCustomCardViewModel>();
            Services.AddTransient<AllCommentViewModel>();
            Services.AddTransient<BaseViewModel>();
            Services.AddTransient<CardsViewModel>();
            Services.AddTransient<DevicesViewModel>();
            Services.AddTransient<HomeViewModel>();
            Services.AddTransient<LoginViewModel>();
            Services.AddTransient<MoreViewModel>();
            #endregion
            #endregion

            #region Pages

            #region LinksPages
            Services.AddTransient<LinksPage>();
            Services.AddTransient<AddLinksPage>();
            #endregion

            #region Share Pages
            Services.AddTransient<ActiveDevicePage>();
            Services.AddTransient<AddCustomCardPage>();
            Services.AddTransient<AddLeadsPage>();
            Services.AddTransient<AllCommentPage>();
            Services.AddTransient<DevicesPage>();
            Services.AddTransient<HomePage>();
            //Services.AddTransient<ImageEditorPage>();
            Services.AddTransient<LoginPage>();
            Services.AddTransient<ScanQrPage>();
            #endregion

            #region MainPopups
            Services.AddTransient<AddAttachmentsPopup>();
            Services.AddTransient<CardOptionPopup>();
            Services.AddTransient<CommentPopup>();
            Services.AddTransient<EditLinkPopup>();
            Services.AddTransient<LeadOptionsPopup>();
            Services.AddTransient<ShareLeadPopup>();
            Services.AddTransient<InsertDevicePopup>();
            Services.AddTransient<CalendlyDetailsPopup>();
            Services.AddTransient<OutLookDetailsPopup>();
            Services.AddTransient<GmailDetailsPopup>();

            #endregion

            #endregion

#if ANDROID

            Services.AddTransient<INotificationManagerService, Cardrly.Platforms.Android.NotificationManagerService>();
            Services.AddSingleton<IAudioStreamService, AndroidAudioService>();
#elif IOS
            Services.AddTransient<INotificationManagerService, Cardrly.Platforms.iOS.NotificationManagerService>();
            Services.AddSingleton<IAudioStreamService, iOSAudioService>();
            Services.AddSingleton<INativeAudioRecorder, iOSAudioRecorder>();
#endif
            Services.AddSingleton<IAudioManager, AudioManager>();
            return Services;
        }

        public static void ControlsBackground()
        {

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
            });

            Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping(nameof(SearchBarHandler), (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                var textField = handler.PlatformView.ValueForKey(new Foundation.NSString("searchField")) as UIKit.UITextField;
                if (textField != null)
                {
                    textField.BackgroundColor = UIKit.UIColor.Clear; // Set text field background color
                    textField.ClipsToBounds = true;
                }
#endif
            });


            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(nameof(PickerHandler), (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
            });

            

            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(nameof(EditorHandler), (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
            });


            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(DatePickerHandler), (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
            });

            Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping(nameof(TimePickerHandler), (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
            });


        }
    }
}

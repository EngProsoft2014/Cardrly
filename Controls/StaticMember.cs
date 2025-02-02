﻿

using Akavache;
using Cardrly.Helpers;
using Cardrly.Pages;
using Cardrly.Resources.Lan;
using Cardrly.Services.Data;
using Cardrly.ViewModels;
using CommunityToolkit.Maui.Core;
using Plugin.Maui.Audio;
using System.Reactive.Linq;

namespace Cardrly.Controls
{
    static class StaticMember
    {
        public static IAudioManager _audioManager;

        #region Const Variables
        public static string SnackBarColor = "#FF7F3E";
        public static string SnackBarTextColor = "#FFFFFF";
        public static int WayOfTab { get; set; } = 0;
        public static int WayOfPhotosPopup { get; set; } = 0;
        public static bool ShowSendOfferBtn { get; set; } = false;
        public static DateTime EndRequestStatic { get; set; }
        #endregion

        #region SnackBar Setting
        [Obsolete]
        public static async void ShowSnackBar(string Message, string BKColor, string TextColor, Action action1)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                
                BackgroundColor = Color.FromHex(BKColor),
                TextColor = Color.FromHex(TextColor),
                ActionButtonTextColor = Color.FromHex(TextColor),
                CornerRadius = new CornerRadius(10),
                Font = Microsoft.Maui.Font.SystemFontOfSize(14),
                ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(14),
                
            };
            string text = Message;
            string actionButtonText = $"{AppResources.msgOk}";
            Action action = action1;
            TimeSpan duration = TimeSpan.FromSeconds(3);

            var snackbar = CommunityToolkit.Maui.Alerts.Snackbar.Make(text, action, actionButtonText, duration, snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }
        #endregion

        public static double Clamp(this double self, double min, double max)
        {
            if (max < min)
            {
                return max;
            }
            else if (self < min)
            {
                return min;
            }
            else if (self > max)
            {
                return max;
            }

            return self;
        }

        public async static Task ClearAllData(IGenericRepository generic)
        {
            ServicesService _service = new ServicesService(generic);

            await App.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgServicedown}", $"{AppResources.msgOk}");

            string LangValueToKeep = Preferences.Default.Get("Lan", "en");
            Preferences.Default.Clear();
            await BlobCache.LocalMachine.InvalidateAll();
            await BlobCache.LocalMachine.Vacuum();
            Preferences.Default.Set("Lan", LangValueToKeep);
            await Application.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(generic, _service,_audioManager)));
        }

        public static async Task<byte[]> GetImageBase64FromUrlAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentNullException(nameof(imageUrl));

            using (HttpClient client = new HttpClient())
            {
                // Fetch image data from the URL
                using (Stream stream = await client.GetStreamAsync(imageUrl))
                {
                    // Convert stream to Base64 string
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        return imageBytes;
                    }
                }
            }
        }
    }
}

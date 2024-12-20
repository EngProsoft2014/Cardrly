

using Akavache;
using Cardrly.Helpers;
using Cardrly.Pages;
using Cardrly.Services.Data;
using Cardrly.ViewModels;
using CommunityToolkit.Maui.Core;
using System.Reactive.Linq;

namespace Cardrly.Controls
{
    static class StaticMember
    {
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
            string actionButtonText = "OK";
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

            await App.Current!.MainPage!.DisplayAlert("Warnaing", "Service is currently down. Please try again later.", "ok");

            string LangValueToKeep = Preferences.Default.Get("Lan", "en");
            Preferences.Default.Clear();
            await BlobCache.LocalMachine.InvalidateAll();
            await BlobCache.LocalMachine.Vacuum();
            Preferences.Default.Set("Lan", LangValueToKeep);
            var vm = new LoginViewModel(generic, _service);
            var page = new LoginPage();
            page.BindingContext = vm;
            await Application.Current!.MainPage!.Navigation.PushAsync(page);
        }
    }
}

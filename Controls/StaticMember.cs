
using Akavache;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.Card;
using Cardrly.Models.Permision;
using Cardrly.Pages;
using Cardrly.Resources.Lan;
using Cardrly.Services;
using Cardrly.Services.AudioStream;
using Cardrly.Services.Data;
using Cardrly.ViewModels;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Controls.UserDialogs.Maui;
using Microsoft.IdentityModel.Tokens;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reactive.Linq;


namespace Cardrly.Controls
{
    static class StaticMember
    {
        //public static IAudioManager _audioManager;
        //public static IAudioStreamService _audioService;
        public static INotificationManagerService notificationManager;
        //public static IPlatformLocationService platformLocationService;

        #region Const Variables
        public static int EmployeesPages { get; set; }
        //static GenericRepository ORep = new GenericRepository();
        //static readonly ServicesService _service = new ServicesService(ORep);
        //static readonly SignalRService _signalRService = new SignalRService(_service);

        //static readonly LocationTrackingService _locationTracking = new LocationTrackingService(_signalRService, platformLocationService, ORep, _service, _audioService);

        public static DateTime SelectedDate { get; set; } = DateTime.UtcNow;
        public static string SnackBarColor = "#FF7F3E";
        public static string SnackBarTextColor = "#FFFFFF";
        public static int WayOfTab { get; set; } = 0;
        public static int WayOfPhotosPopup { get; set; } = 0;
        public static bool ShowSendOfferBtn { get; set; } = false;
        public static DateTime EndRequestStatic { get; set; }

        public static string AzureMeetingAiSekrtKey { get; set; } = "";

        public static List<PermissionsValues> LstPermissions = new List<PermissionsValues>();
        public static List<UpdateVersionModel> LstUpdateVersions = new List<UpdateVersionModel>();
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

        //public async static Task ClearAllData(IGenericRepository generic)
        //{
        //    ServicesService _service = new ServicesService(generic);

        //    await App.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgServicedown}", $"{AppResources.msgOk}");

        //    await StaticMember.DeleteUserSession(generic, _service);

        //    string LangValueToKeep = Preferences.Default.Get("Lan", "en");
        //    bool RememberMe = Preferences.Default.Get<bool>(ApiConstants.rememberMe, false);
        //    string RememberMeUserName = Preferences.Default.Get<string>(ApiConstants.rememberMeUserName, string.Empty);
        //    string RememberPassword = Preferences.Default.Get<string>(ApiConstants.rememberMePassword, string.Empty);

        //    Preferences.Default.Clear();
        //    await BlobCache.LocalMachine.InvalidateAll();
        //    await BlobCache.LocalMachine.Vacuum();

        //    Preferences.Default.Set("Lan", LangValueToKeep);
        //    Preferences.Default.Set(ApiConstants.rememberMe, RememberMe);
        //    Preferences.Default.Set(ApiConstants.rememberMeUserName, RememberMeUserName);
        //    Preferences.Default.Set(ApiConstants.rememberMePassword, RememberPassword);
        //    await Application.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(generic, _service, _signalRService, _audioService, _locationTracking)));
        //}

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

        public static bool CheckPermission(string PermissionTag)
        {
            if (StaticMember.LstPermissions.Any(P => P.ClaimValue == PermissionTag))
                return true;

            return false;
        }

        public static string? GuidKeyFromToken(string token)
        {
            var tokenhandler = new JwtSecurityTokenHandler();
            var symmetricSecurityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(ApiConstants.KeyJwtInApi));
            try
            {
                tokenhandler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero, //do not stop token when time still work
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                return (jwtToken.Claims.FirstOrDefault(x => x.Type == "GuidKey")?.Value!);
            }
            catch
            {
                return null;
            }
        }

        public static async Task DeleteUserSession(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            try
            {
                string UserToken = await service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    await GenericRep.PostEAsync(ApiConstants.UserSessionDeleteApi, UserToken);
                }
            }
            catch (Exception)
            {

            }
        }


        public static async Task<string> GetDeviceId()
        {
            string deviceID = string.Empty;
#if ANDROID

            deviceID = Android.Provider.Settings.Secure.GetString(Platform.CurrentActivity!.ContentResolver, Android.Provider.Settings.Secure.AndroidId)!;

#elif IOS
            deviceID = UIKit.UIDevice.CurrentDevice.IdentifierForVendor.ToString();
#endif
            return deviceID;
        }


        public static async Task<string> GetAddressFromCurrentLocation()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(1));
            var location = await Geolocation.GetLocationAsync(request);

            if (location == null)
                return "No location available";

            // Reverse geocode: convert lat/long to placemarks (addresses)
            var placemarks = await Geocoding.GetPlacemarksAsync(location);

            var placemark = placemarks?.FirstOrDefault();
            if (placemark != null)
            {
                // You can build a full address string
                return $"{placemark.Thoroughfare} {placemark.SubThoroughfare}, " +
                       $"{placemark.Locality}, {placemark.AdminArea}, " +
                       $"{placemark.CountryName}, {placemark.PostalCode}";
            }
            else
            {
                return "Address not found";
            }
        }

    }
}

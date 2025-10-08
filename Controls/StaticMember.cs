
using Akavache;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.Card;
using Cardrly.Models;
using Cardrly.Models.Card;
using Cardrly.Models.Permision;
using Cardrly.Pages;
using Cardrly.Resources.Lan;
using Cardrly.Services;
using Cardrly.Services.AudioStream;
using Cardrly.Services.Data;
using Cardrly.ViewModels;
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
        public static IAudioManager _audioManager;
        public static IAudioStreamService _audioService;
        public static INotificationManagerService notificationManager;
        //public static int TabIndex = 0;

        #region Const Variables
        public static int EmployeesPages { get; set; }
        static Helpers.GenericRepository ORep = new Helpers.GenericRepository();
        static readonly ServicesService _service = new ServicesService(ORep);
        public static ObservableCollection<CardResponse> LstWorkingEmployeesStatic { get; set; }
        public static DateTime SelectedDate { get; set; } = DateTime.Now;
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

        public async static Task ClearAllData(IGenericRepository generic)
        {
            ServicesService _service = new ServicesService(generic);

            await App.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgServicedown}", $"{AppResources.msgOk}");

            string LangValueToKeep = Preferences.Default.Get("Lan", "en");
            Preferences.Default.Clear();
            await BlobCache.LocalMachine.InvalidateAll();
            await BlobCache.LocalMachine.Vacuum();
            Preferences.Default.Set("Lan", LangValueToKeep);
            await Application.Current!.MainPage!.Navigation.PushAsync(new LoginPage(new LoginViewModel(generic, _service, _audioService)));
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

        //Get Working Employees
        public async static Task GetWorkingEmployees(string AccountId, string ScheduleDate)
        {
            UserDialogs.Instance.ShowLoading();

            //string json = await Helpers.Utility.CallWebApi("api/Employee/GetEmpWorking?" + "AccountId=" + AccountId + "&" + "ScheduleDate=" + ScheduleDate);

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                string UserToken = await _service.UserToken();

                var json = await ORep.GetAsync<ObservableCollection<CardResponse>>("api/Employee/GetEmpWorking?" + "AccountId=" + AccountId + "&" + "ScheduleDate=" + ScheduleDate, UserToken);

                if (json != null)
                {
                    //List<EmployeeModel> Employee = JsonConvert.DeserializeObject<List<EmployeeModel>>(json);

                    //LstWorkingEmployeesStatic = new ObservableCollection<EmployeeModel>(Employee);
                    LstWorkingEmployeesStatic = json;
                }
            }

            UserDialogs.Instance.HideHud();
        }
    }
}

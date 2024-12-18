namespace Cardrly.Constants
{
    public class ApiConstants
    {
        public static string BaseApiUrl;

        //public const string syncFusionLicence = "Ngo9BigBOggjHTQxAR8/V1NCaF1cWWhIfkxwWmFZfVpgfF9GZ1ZUTGYuP1ZhSXxXdkxhWn5Xc3BQRmVbUUE="; //Version= 24.x.x
        public const string syncFusionLicence = "Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXZcc3VcRWldUkN0V0Y="; //Version= 27.x.x


        #region Preferences Keys
        // Preferences Key
        public static string userid = "userid";
        public static string email = "email";
        public static string username = "username";
        public static string password = "password";
        public static string userPermision = "userPermision";
        public static string userCategory = "userCategory"; //1 = system , 2 = Travel Agency , 3 = Distributor
        public static string AccountId = "AccountId";
        //End Preferences Key 
        #endregion

        #region Login & Register Api 
        public static string LoginApi = "api/ApplicationUser/Login";
        public static string RegisterApi = "api/ApplicationUser/Register";
        public static string ForgetPasswordApi = "api/ApplicationUser/forget-Password";
        public static string ResendConfirmemailApi = "api/ApplicationUser/resend-confirm-email";
        #endregion

        #region Account 
        public static string AccountInfoApi = "Account/{0}";
        public static string AccountInfoUpdateApi = "Account/{0}";
        #endregion

        #region Account Links 
        public static string AccountLinksAllApi = "Account/{0}/AccountLink";
        public static string AccountLinksCurrentApi = "Account/{0}/AccountLink/current";
        public static string AccountLinksInfoApi = "Account/{0}/AccountLink/{1}";
        public static string AccountLinksAddApi = "Account/{0}/AccountLink";
        public static string AccountLinksUpdateApi = "Account/{0}/AccountLink/{1}";
        public static string AccountLinksDeleteApi = "Account/{0}/AccountLink/{1}/Delete";
        public static string AccountLinksToggleActiveApi = "Account/{0}/AccountLink/{1}/ToggleActive";

        #endregion

        #region Users 
        public static string UserProfileApi = "Users";
        public static string UserChangePasswordApi = "Users/ChangePassword";
        public static string UsersAccountApi = "Users/Account/{0}";
        public static string DeleteUserApi = "Users/Account/{0}/{1}/Delete";
        public static string UserPermissionGetApi = "Users/User/{0}";
        public static string ToggleUserPermissionApi = "Users/ChoosenPermission/{0}/{1}";
        public static string ToggleUserDisabledApi = "Users/UserDisabled/{0}";
        #endregion

        #region Cards
        public static string CardGetAllApi = "Account/";
        public static string CardGetApi = "Account/{0}/Card/{1}";
        public static string CardAddApi = "Account/";
        public static string CardUpdateApi = "Account/";
        public static string CardDeleteApi = "Account/";
        public static string CardToggleApi = "Account/{0}/Card/{1}/ToggleActive";
        public static string CardGetDetailsAllApi = "CardDetails/";
        #endregion

        #region CardLinks
        public static string CardLinkGetAllApi = "Account/{0}/Card/{1}/CardLink";
        public static string CardLinkGetApi = "Account/{0}/Card/{1}/CardLink/{2}";
        public static string CardLinkAddApi = "Account/{0}/Card/{1}/CardLink";
        public static string CardLinkUpdateApi = "Account/";
        public static string CardLinkDeleteApi = "Account/";
        public static string CardLinkToggleApi = "Account/";
        #endregion
    }
}


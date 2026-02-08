namespace Cardrly.Constants
{
    public class ApiConstants
    {
        public static string BaseApiUrl;

        //public const string syncFusionLicence = "Ngo9BigBOggjHTQxAR8/V1NCaF1cWWhIfkxwWmFZfVpgfF9GZ1ZUTGYuP1ZhSXxXdkxhWn5Xc3BQRmVbUUE="; //Version= 24.x.x
        public const string syncFusionLicence = "Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXZcc3VcRWldUkN0V0Y="; //Version= 27.x.x

        public static string KeyJwtInApi = "iwGuW8GAcuMJXlH6oLLeos3vhIcvd8sR";


        #region Preferences Keys
        // Preferences Key
        public static string userid = "userid";
        public static string ownerId = "ownerid";
        public static string cardId = "cardid";
        public static string email = "email";
        public static string username = "username";
        public static string password = "password";
        public static string userPermision = "userPermision";
        public static string userCategory = "userCategory"; //1 = system , 2 = Travel Agency , 3 = Distributor
        public static string AccountId = "AccountId";
        public static string TimeSheetId = " TimeSheetId";
        public static string DeviceId = "deviceId";
        public static string AccountName = "AccountName";
        public static string ExpireDate = "ExpireDate";
        public static string GuidKey = "GuidKey";

        public static string rememberMe = "rememberMe";
        public static string rememberMeUserName = "rememberMeUserName";
        public static string rememberMePassword = "rememberMePassword";
        public static string shortcutIcons = "shortcuticons";
        public static string isFirstRun = "isFirstRun";
        public static string isTimeSheetCheckout = "isTimeSheetCheckout";
        //End Preferences Key 
        #endregion

        #region Login & Register Api 
        public static string LoginApi = "api/ApplicationUser/Login";
        public static string RegisterApi = "api/ApplicationUser/Register";
        public static string ForgetPasswordApi = "api/ApplicationUser/forget-Password";
        public static string ResendConfirmemailApi = "api/ApplicationUser/resend-confirm-email";
        #endregion

        #region Account 
        public static string AccountInfoApi = "Account/";
        public static string AccountInfoUpdateApi = "Account/{0}";
        #endregion

        #region Account Links 
        public static string AccountLinksAllApi = "Account/{0}/AccountLink";
        public static string AccountLinksCurrentApi = "Account/";
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
        public static string CardGetByUserApi = "Account/";
        public static string CardGetApi = "Account/";
        public static string CardGetCardDashBoardApi = "Account/";
        public static string LeadGetScanCardApi = "Account/";
        public static string CardGetAllOrSingleApi = "Account/{0}/Card/GetAllOrSingle";
        public static string CardAddApi = "Account/";
        public static string CardUpdateApi = "Account/";
        public static string CardDeleteApi = "Account/";
        public static string CardToggleApi = "Account/{0}/Card/{1}/ToggleActive";
        public static string CardGetDetailsAllApi = "CardDetails/";
        #endregion

        #region CardLinks
        public static string CardLinkGetAllApi = "Account/{0}/Card/{1}/CardLink";
        public static string CardLinkGetApi = "Account/{0}/Card/{1}/CardLink/{2}";
        public static string CardLinkAddApi = "Account/";
        public static string CardLinkUpdateApi = "Account/";
        public static string CardLinkDeleteApi = "Account/";
        public static string CardLinkToggleApi = "Account/";
        public static string CardLinkSortApi = "Account/";
        #endregion

        #region Leads
        public static string LeadGetAllApi = "Account/";
        public static string LeadGetApi = "Account/{0}/Lead/{1}";
        public static string LeadAddApi = "Account/";
        public static string LeadUpdateApi = "Account/";
        public static string LeadDeleteApi = "Account/";
        public static string LeadToggleApi = "Account/";
        #endregion

        #region LeadAssign
        public static string LeadAssignGetAllApi = "Account/";
        public static string LeadAssignAddApi = "Account/";
        public static string LeadAssignDeleteApi = "Account/";
        public static string LeadAssignToggleApi = "Account/";
        #endregion

        #region LeadComment
        public static string LeadCommentGetAllApi = "Account/";
        public static string LeadCommentAddApi = "Account/";
        public static string LeadCommentDeleteApi = "Account/";
        #endregion

        #region LeadCategory 
        public static string LeadCategoryAllApi = "Account/{0}/LeadCategory";
        public static string LeadCategoryCurrentApi = "Account/";
        public static string LeadCategoryInfoApi = "Account/{0}/LeadCategory/{1}";
        public static string LeadCategoryAddApi = "Account/{0}/LeadCategory";
        public static string LeadCategoryUpdateApi = "Account/{0}/LeadCategory/{1}";
        public static string LeadCategoryDeleteApi = "Account/{0}/LeadCategory/{1}/Delete";
        public static string LeadCategoryToggleActiveApi = "Account/{0}/LeadCategory/{1}/ToggleActive";
        #endregion

        #region Devices
        public static string DevicesGetByCardApi = "Account/";
        public static string DevicesAddApi = "Account/";
        public static string DevicesDeleteApi = "Account/"; 
        public static string DevicesGetAllApi = "Account/";
        #endregion

        #region Calender
        public static string CalnderGetByApi = "Account/";
        public static string CalendarAddEventsApi = "Account/";
        public static string CalendarDeleteEventsApi = "Account/";
        #endregion

        #region Permission

        // == Account == 
        public const string GetAccount = "Account:Get";
        public const string AddAccount = "Account:Add";
        public const string UpdateAccount = "Account:Update";
        public const string DeleteAccount = "Account:Delete";

        // == AccountLinks == 
        public const string GetAccountLinks = "AccountLinks:Get";
        public const string AddAccountLinks = "AccountLinks:Add";
        public const string UpdateAccountLinks = "AccountLinks:Update";
        public const string DeleteAccountLinks = "AccountLinks:Delete";

        // == Cards == 
        public const string GetCards = "Cards:Get";
        public const string AddCards = "Cards:Add";
        public const string UpdateCards = "Cards:Update";
        public const string DeleteCards = "Cards:Delete";

        // == Leads ==
        public const string GetLeads = "Leads:Get";
        public const string AddLeads = "Leads:Add";
        public const string UpdateLeads = "Leads:Update";
        public const string DeleteLeads = "Leads:Delete";
        public const string ScanCardLeads = "Leads:ScanCard";

        // == Users == 
        public const string GetUsers = "Users:Get";
        public const string AddUsers = "Users:Add";
        public const string UpdateUsers = "Users:Update";
        public const string DeleteUsers = "Users:Delete";

        // == Calendar ==
        public const string GetCalendar = "Calendar:Get";
        public const string GetEventsCalendar = "Calendar:GetEvents";
        public const string UpdateCalendar = "Calendar:Update";

        // == Devices == 
        public const string GetDevices = "Devices:Get";
        public const string AddDevices = "Devices:Add";
        public const string DeleteDevices = "Devices:Delete";

        // == LeadCategory ==
        public const string GetLeadCategory = "LeadCategory:Get";
        public const string AddLeadCategory = "LeadCategory:Add";
        public const string UpdateLeadCategory = "LeadCategory:Update";
        public const string DeleteLeadCategory = "LeadCategory:Delete";

        // == MarketingCampaign ==
        public const string GetMarketingCampaign = "MarketingCampaign:Get";
        public const string GetAIMarketingCampaign = "MarketingCampaign:GetAI";
        public const string SentMarketingCampaign = "MarketingCampaign:Sent";
        public const string AddMarketingCampaign = "MarketingCampaign:Add";
        public const string UpdateMarketingCampaign = "MarketingCampaign:Update";
        public const string DeleteMarketingCampaign = "MarketingCampaign:Delete";

        // == Marketing == 
        public const string GetMarketing = "Marketing:Get";
        public const string UpdateMarketing = "Marketing:Update";

        // == Marketing == 
        public const string GetStripe = "Stripe:Get";

        // == MeetingAi == 
        public const string GetMeetingAi = "MeetingAi:Get";

        // == TimeSheet ==
        public const string GetTimeSheet = "TimeSheet:Get";
        public const string UpdateTimeSheet = "TimeSheet:Update";
        public const string GetTimeSheetTracking = "TimeSheet:GetTracking";
        public const string SendLocationTimeSheet = "TimeSheet:SendLocation";
        public const string GetHistoryLocationTimeSheet = "TimeSheet:GetHistoryLocation";
        #endregion

        #region Stripe
        public static string StripeGetBillingApi = "Stripe/";
        #endregion

        #region UserSessionDeleteApi
        public static string UserSessionDeleteApi = "UserSession/Delete";
        #endregion

        #region MeetingAiApi
        public static string CreateMeetingAiActionApi = "api/MeetingAiAction/AddMeetingAiAction/";
        public static string GetAllMeetingAiActionApi = "api/MeetingAiAction/GetAll/";
        public static string GetMeetingAiActionInfoApi = "api/MeetingAiAction/GetAllInfo/";
        public static string PostMeetingAiScriptTextToPDFApi = "api/MeetingAiAction/GeneratePdf";
        public static string DeleteMeetingAiActionApi = "api/MeetingAiAction/Delete/";

        public static string AddMeetingAiActionRecordApi = "api/MeetingAiActionRecord/AddMeetingAiActionRecord/";
        public static string DeleteMeetingAiActionRecordApi = "api/MeetingAiActionRecord/Delete/";
        public static string AddMeetingAiActionRecordAnalyzeApi = "api/MeetingAiActionRecordAnalyze/AddMeetingAiActionRecordAnalyze/";



        #endregion

        #region TimeSheetApi
        public static string GetByDateTimeSheetApi = "api/TimeSheet/GetByDate/";
        public static string GetByCardIdTimeSheetApi = "api/TimeSheet/GetByCardId/"; 
        public static string AddTimeSheetApi = "api/TimeSheet/AddTimeSheet/";
        public static string UpdateTimeSheetApi = "api/TimeSheet/UpdateTimeSheet/";
        public static string GetEmpWorkingTimeSheetApi = "api/TimeSheet/GetEmpWorking/";
        public static string GetLastLocationTimeSheetApi = "api/TimeSheet/GetLastEmployeeLocation/";
        public static string GetDeviceIdTimeSheetApi = "api/TimeSheetEmployeeBranch/GetDeviceId/";

        public static string GetAllBranchesTimeSheetApi = "api/TimeSheetBranch/GetAllForHistoryTracking/";
        public static string GetTimeSheetsByEmployeeTimeSheetApi = "api/TimeSheet/GetTimeSheetsByEmployee/";
        public static string GetEmployeeLocationsTimeSheetApi = "api/TimeSheet/GetEmployeeLocations/";


        #endregion


        #region FCM_Notifications
        public static string SendNotifyToOwner = "api/ApplicationUser/Notify/Owner/";
        public static string RefrshFCMToken = "api/ApplicationUser/RefreshToken";
        #endregion
    }
}
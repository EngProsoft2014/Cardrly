

using Cardrly.Enums;

namespace Cardrly.Models.ApplicationUser
{
    public class ApplicationUserProfileResponse
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public UserCategory UserCategory { get; set; }
        public EnumUserPermission UserPermision { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyUrlLogo { get; set; }
    }
}

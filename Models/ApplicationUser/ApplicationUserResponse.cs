using Cardrly.Mode_s.Account;
using Cardrly.Models;
using Cardrly.Models.Permision;

namespace Cardrly.Mode_s.ApplicationUser
{
    public class ApplicationUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? UserCategory { get; set; }
        public List<PermissionsValues>? Permissions { get; set; }
        public string? Token { get; set; }
        public int ExpiresIn { get; set; }
        public bool IsDisabled { get; set; }
        public bool EmailConfirmed { get; set; }
        public string AccountId { get; set; } = string.Empty;
        public AccountResponse? Account { get; set; }
        public List<UpdateVersionModel> VersionAppMobile { get; set; } = [];
    }
}

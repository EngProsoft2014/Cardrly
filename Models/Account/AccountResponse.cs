using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Mode_s.Account
{
    public class AccountResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Logo { get; set; }
        public string? UrlLogo { get; set; }
        public DateOnly? StartDateAcc { get; set; }
        public DateOnly? ExpireDateAcc { get; set; }
        public int DayOperationAcc { get; set; }
        public int DayOperationExpireAcc { get; set; }
        public int? CountUsers { get; set; }
        public int? CountCards { get; set; }
        public int? CurrentCountUsers { get; set; }
        public int? CurrentCountCards { get; set; }
    }
}

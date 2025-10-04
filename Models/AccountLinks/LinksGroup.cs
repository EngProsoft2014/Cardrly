using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.AccountLinks
{
    public class LinksGroup : List<AccountLinkResponse>
    {
        public string? GroupName { get; set; }
        public LinksGroup(string Name,List<AccountLinkResponse> linkResponses) : base(linkResponses)
        {
            GroupName = Name;
        }
    }
}

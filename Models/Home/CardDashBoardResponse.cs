using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Home
{
    public class CardDashBoardResponse
    {
        public int CountLeads { get; set; }
        public int CountLinkTap { get; set; }
        public int CountViewCard { get; set; }
        public int CountDownloadContacts { get; set; }
        public List<ClickCardSummaryResponse> clickCardSummaries { get; set; } = [];
        public List<ClickCardLinkSummaryResponse> clickCardLinkSummaries { get; set; } = [];
        public List<ClickCardSummaryOSResponse> clickCardLinkSummariesOS { get; set; } = [];

    }
}

using Cardrly.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Home
{
    public class ClickCardLinkSummaryResponse
    {
        public string ImgCardLink { get; set; }
        public string ImgCardLinkVM { get{ return !string.IsNullOrEmpty(ImgCardLink) ? Utility.ServerUrl + ImgCardLink : "https://app.cardrly.com/images/defaultcardlink.jpg"; } }
        public string LinkName { get; set; } = default!;
        public string PoplUser { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public int CountClickCardLink { get; set; } = default!;
    }
}

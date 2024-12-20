using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Home
{
    public class ClickCardLinkSummaryResponse
    {
        public string ImgCardLink { get; set; } = default!;
        public string LinkName { get; set; } = default!;
        public string PoplUser { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public int CountClickCardLink { get; set; } = default!;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.CardLink
{
    public class CardLinkSortRequest
    {
        public string Id { get; set; } = default!;
        public int SortNumber { get; set; }
    }
}

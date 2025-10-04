using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Home
{
    public class ClickCardSummaryResponse
    {
        public int CountClickCard { get; set; } = default!;
        public string DateClick { get; set; } = default!;
    }
}

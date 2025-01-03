using Syncfusion.Maui.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models
{
    public class CalendarModel
    {
        public CalendarDateRange SelectedRange { get; set; }

        public DateTime? StartDate { get; set; } = DateTime.UtcNow.Date;

        public DateTime? EndDate { get; set; } = DateTime.UtcNow.Date.AddDays(1);
    }
}

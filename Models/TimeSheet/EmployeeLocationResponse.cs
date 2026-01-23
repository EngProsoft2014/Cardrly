using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.TimeSheet
{
    public class EmployeeLocationResponse
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string TimeSheetId { get; set; }
        public string EmployeeId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreateDate { get; set; }
        public TimeSpan Time { get; set; }
    }
}

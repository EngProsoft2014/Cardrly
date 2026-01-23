using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models
{
    public class DataMapsModel
    {
        public Empoylee EmpData { get; set; }
        public string Id { get; set; } = new Guid().ToString();
        public string AccountId { get; set; }
        public string TimeSheetId { get; set; }
        public string EmployeeId { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public DateTime CreateDate { get; set; }
        public TimeSpan Time { get; set; }
        public Location MPosition { get { return new Location(Lat == null ? 0 : Lat, Long == null ? 0 : Long); } }

    }

    public class Empoylee
    {
        public string Tracking_id { get; set; }
        public string AccountId { get; set; }
        public string BranchId { get; set; }
        public string EmployeeId { get; set; }
        public double lat { get; set; }
        public double log { get; set; }
        public DateTime date { get; set; }
        public TimeSpan time { get; set; }
    }
}

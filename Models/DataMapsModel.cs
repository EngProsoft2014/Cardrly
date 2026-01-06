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
        public int Id { get; set; } = 0;
        public string BranchId { get; set; }
        public string EmployeeId { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public string CreateDate { get; set; }
        public string Time { get; set; }
        //public Location MPosition { get; set; }
        public Location MPosition { get { return new Location(Lat == null ? 0 : double.Parse(Lat), Long == null ? 0 : double.Parse(Long)); } }

    }

    public class Empoylee
    {
        public int Tracking_id { get; set; }
        public string BranchId { get; set; }
        public string EmployeeId { get; set; }
        public string lat { get; set; }
        public string log { get; set; }
        public string date { get; set; }
        public string time { get; set; }
    }
}

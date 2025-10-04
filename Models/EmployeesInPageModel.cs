using Cardrly.Models.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models
{
    public class EmployeesInPageModel
    {
        public List<CardResponse> EmployeesInPage { get; set; }
        public int Pages { get; set; }
        public int CurrentPage { get; set; }
    }
}

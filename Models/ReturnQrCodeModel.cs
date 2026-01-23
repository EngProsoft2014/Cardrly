using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cardrly.Models
{
    public class ReturnQrCodeModel
    {
        public string matchValue { get; set; }
        public string scanUriValue { get; set; }
        public string scanUriRedirectValue { get; set; }
        public bool isManualQrCode { get; set; }

    }
}

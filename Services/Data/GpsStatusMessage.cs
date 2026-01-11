using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Services.Data
{
    public class GpsStatusMessage
    {
        public bool IsEnabled { get; }
        public GpsStatusMessage(bool isEnabled) => IsEnabled = isEnabled;
    }
}

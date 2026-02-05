using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Services
{
    public interface IPlatformLocationService
    {
        void StartBackgroundTracking(string employeeId);
        void StopBackgroundTracking();
        void ResetBackgroundTracking();
    }
}

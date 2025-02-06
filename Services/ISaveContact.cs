using Cardrly.Models.Lead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Services
{
    public interface ISaveContact
    {
        public Task SaveContactMethod(LeadResponse contact);
    }
}

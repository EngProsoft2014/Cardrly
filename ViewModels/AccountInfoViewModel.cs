using Cardrly.Helpers;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class AccountInfoViewModel : BaseViewModel
    {
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        public AccountInfoViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;

        }

    }
}

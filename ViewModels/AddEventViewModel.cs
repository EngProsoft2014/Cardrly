using Cardrly.Helpers;
using Cardrly.Models.Calendar;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class AddEventViewModel : BaseViewModel
    {
        #region Properties
        [ObservableProperty]
        CalendarGmailRequest request = new CalendarGmailRequest();
        #endregion
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public AddEventViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
        }
        #endregion

        [RelayCommand]
        public async Task SaveClick(CalendarGmailRequest gmailRequest)
        {

        }
    }
}

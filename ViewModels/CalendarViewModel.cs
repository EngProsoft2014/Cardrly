using Cardrly.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class CalendarViewModel :BaseViewModel
    {
        #region Prop
        [ObservableProperty]
        int isPassed = 0; //0 => out coming - 1 => passed 
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public CalendarViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
        }
        #endregion

        #region RelayCommand
        [RelayCommand]
        async Task OutComingClick()
        {
            IsPassed = 0;
        }

        [RelayCommand]
        async Task PassedClick()
        {
            IsPassed = 1;
        } 
        #endregion
    }
}

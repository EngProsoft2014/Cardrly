using Cardrly.Helpers;
using Cardrly.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class ActiveDeviceViewModel :BaseViewModel
    {
        [ObservableProperty]
        ObservableCollection<ActivateDeviceModel> deviceModels = new ObservableCollection<ActivateDeviceModel>();
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public ActiveDeviceViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
            Init();
        }
        #endregion

        void Init()
        {
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "Card",
                Image = "",
                Type = "FontIconBrand"
            });
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "Band",
                Image = "",
                Type = "FontIconSolid"
            });
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "Display",
                Image = "",
                Type = "FontIconSolid"
            });
            DeviceModels.Add(new ActivateDeviceModel
            {
                Name = "KeyChain",
                Image = "",
                Type = "FontIconSolid"
            });
        }
    }
}

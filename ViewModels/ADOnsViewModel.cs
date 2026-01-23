using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Helpers;
using Cardrly.Pages;
using Cardrly.Pages.MeetingsScript;
using Cardrly.Pages.TrackingPages;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using Cardrly.Services.Data;
using Cardrly.ViewModels.MeetingsAi;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class ADOnsViewModel : BaseViewModel
    {
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        readonly SignalRService _signalRService;
        readonly LocationTrackingService _locationTracking;
        #endregion

        private readonly IAudioStreamService _audioService;

        [ObservableProperty]
        bool isShowMeetingSCript;

        [ObservableProperty]
        bool isShowTimeSheet;

        public ADOnsViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service, SignalRService signalRService, IAudioStreamService audioService, LocationTrackingService locationTracking)
        {
            Rep = GenericRep;
            _service = service;
            _signalRService = signalRService;
            _audioService = audioService;
            _locationTracking = locationTracking;
            IsShowMeetingSCript = StaticMember.CheckPermission(ApiConstants.GetMeetingAi) == true ? true : false;
            IsShowTimeSheet = StaticMember.CheckPermission(ApiConstants.GetHistoryLocationTimeSheet) == true ? true : false;
        }


        [RelayCommand]
        async Task MeetingsScriptClick()
        {
            UserDialogs.Instance.ShowLoading();
            if (StaticMember.CheckPermission(ApiConstants.GetMeetingAi))
            {
                await App.Current!.MainPage!.Navigation.PushAsync(new NotesScriptPage(new NotesScriptViewModel(Rep, _service, _audioService)));
            }
            else
            {
                var toast = Toast.Make($"{AppResources.mshPermissionToViewData}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            UserDialogs.Instance.HideHud();
        }

        [RelayCommand]
        async Task TimeSheetClick()
        {
            if (StaticMember.CheckPermission(ApiConstants.GetTimeSheet))
            {
                UserDialogs.Instance.ShowLoading();
                await App.Current!.MainPage!.Navigation.PushAsync(new TimeSheetPage(new TimeSheetViewModel(Rep, _service, _signalRService, _locationTracking)));
                UserDialogs.Instance.HideHud();
            }
            else
            {
                var toast = Toast.Make($"{AppResources.mshPermissionToViewData}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }
    }
}

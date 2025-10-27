using Cardrly.Helpers;
using Cardrly.Models.MeetingAiAction;
using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.Pages.MeetingsScript;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.ViewModels.MeetingsAi
{
    public partial class MeetingSettingViewModel : BaseViewModel
    {
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        [ObservableProperty]
        ObservableCollection<ScriptTypeModel> scriptTypes = new();

        [ObservableProperty]
        ScriptTypeModel selectedScriptType = new();

        [ObservableProperty]
        string selectedLanguage = string.Empty;

        public IAudioStreamService _audioService;

        MeetingAiActionInfoResponse _meetingInfoModel;

        public MeetingSettingViewModel(MeetingAiActionInfoResponse meetingInfoModel, IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioStreamService audioService)
        {
            Rep = GenericRep;
            _service = service;
            _audioService = audioService;
            _meetingInfoModel = meetingInfoModel;

            ScriptTypes.Add(new ScriptTypeModel { Id = 1, Name = "Simple Script" });
            ScriptTypes.Add(new ScriptTypeModel { Id = 2, Name = "Meeting Script" });
        }

        [RelayCommand]
        public async Task GoToRecordPage()
        {
            IsEnable = false;
            //if (string.IsNullOrEmpty(SelectedLanguage))
            //{
            //    var toast = Toast.Make(AppResources.msgRequiredFieldLanguage, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
            //    await toast.Show();
            //}
            if (SelectedScriptType == null || SelectedScriptType.Id == 0)
            {
                var toast = Toast.Make(AppResources.msgRequiredFieldScriptType, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
            else
            {
                await MopupService.Instance.PopAsync();
                await App.Current!.MainPage!.Navigation.PushAsync(new RecordPage(new RecordViewModel(_meetingInfoModel, SelectedScriptType, SelectedLanguage, Rep, _service, _audioService)));
            }

            IsEnable = true;
        }
    }
}

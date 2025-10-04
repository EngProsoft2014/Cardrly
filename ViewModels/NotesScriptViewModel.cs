
using Azure.AI.TextAnalytics;
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.MeetingAiAction;
using Cardrly.Pages;
using Cardrly.Pages.MeetingsScript;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.ViewModels
{
    public partial class NotesScriptViewModel : BaseViewModel
    {
        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion


        [ObservableProperty]
        bool isRecording;
        [ObservableProperty]
        bool isScriptBtn;
        [ObservableProperty]
        bool isShowStopBtn;

        [ObservableProperty]
        string durationDisplay = "00:00";

        [ObservableProperty]
        string noteScript;

        [ObservableProperty]
        ObservableCollection<AudioItem> audioSources = new();

        [ObservableProperty]
        MeetingAiActionResponse meetingModel;

        [ObservableProperty]
        ObservableCollection<MeetingAiActionResponse> lstMeetingModel;

        public IAudioRecorder recorder { get; set; }


        private readonly IAudioStreamService _audioService;

        public NotesScriptViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioStreamService audioService)
        {
            Rep = GenericRep;
            _service = service;
            _audioService = audioService;

            Init();
        }


        void Init()
        {
            LstMeetingModel = new ObservableCollection<MeetingAiActionResponse>();
            Task.WhenAll(GetAllMeetings());

            //AddMeeting
            MessagingCenter.Subscribe<NotesScriptViewModel, bool>(this, "AddMeeting", async (sender, message) =>
            {
                if (true)
                {
                    await GetAllMeetings();
                }
            });
        }

        async Task GetAllMeetings()
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<ObservableCollection<MeetingAiActionResponse>>($"{ApiConstants.GetAllMeetingAiActionApi}{AccId}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    LstMeetingModel = json;
                }
            }
            IsEnable = true;
        }

        [RelayCommand]
        public async Task CreateNewMeeting()
        {
            string Pass = await App.Current!.MainPage!.DisplayPromptAsync(AppResources.Info, AppResources.msgEnterTitleofMeeting, AppResources.msgOk, AppResources.btnCancel);

            if (!string.IsNullOrEmpty(Pass) && Pass.Length > 3)
            {

                IsEnable = false;
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    MeetingAiActionRequest obj = new MeetingAiActionRequest { title = Pass };
                    UserDialogs.Instance.ShowLoading();
                    var json = await Rep.PostTRAsync<MeetingAiActionRequest, MeetingAiActionResponse>($"{ApiConstants.CreateMeetingAiActionApi}{AccId}", obj, UserToken);
                    UserDialogs.Instance.HideHud();
                    if (json.Item1 != null)
                    {
                        var toast = Toast.Make(AppResources.msgSuccessfullyforaddmeeting, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();

                        MessagingCenter.Send(this, "AddMeeting", true);

                        await GoToMeetingDetails(json.Item1);
                    }
                    else
                    {
                        var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Key + " " + json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                IsEnable = true;
            }
            else
            {
                var toast = Toast.Make(AppResources.msgEntermeetingnameisvalid, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                await toast.Show();
            }
        }


        [RelayCommand]
        public async Task GoToMeetingDetails(MeetingAiActionResponse model)
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<MeetingAiActionInfoResponse>($"{ApiConstants.GetMeetingAiActionInfoApi}{model.Id}", UserToken);
               
                if (json != null)
                {
                    await App.Current!.MainPage!.Navigation.PushAsync(new NoteScriptDetailsPage(new NotesScriptDetailsViewModel(json, Rep, _service, _audioService)));
                }
                UserDialogs.Instance.HideHud();
            }
            IsEnable = true;
        }

        [RelayCommand]
        public async Task DeleteMeeting(MeetingAiActionResponse model)
        {
            bool result = await App.Current!.MainPage!.DisplayAlert(AppResources.msgDeleteMeeting, AppResources.msgAreyousuredeletemeeting, AppResources.msgYes, AppResources.msgNo);
            if (result)
            {
                IsEnable = false;
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    UserDialogs.Instance.ShowLoading();
                    string AccId = Preferences.Default.Get(ApiConstants.AccountId, "");
                    string response = await Rep.PostEAsync($"{ApiConstants.DeleteMeetingAiActionApi}{AccId}/{model.Id}", UserToken);
                    UserDialogs.Instance.HideHud();
                    if (response == "")
                    {
                        UserDialogs.Instance.ShowLoading();
                        await GetAllMeetings();
                        UserDialogs.Instance.HideHud();
                    }
                    else
                    {
                        var toast = Toast.Make(AppResources.msgTheMeetinghasnotbeendeleted, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                IsEnable = true;

            }
        }
    }
}

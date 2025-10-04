
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.MeetingAiAction;
using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.Models.MeetingAiActionRecordAnalyze;
using Cardrly.Pages;
using Cardrly.Pages.MeetingsScript;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Cardrly.ViewModels
{
    public partial class NotesScriptDetailsViewModel : BaseViewModel
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

        MeetingAiActionRecordResponse? _currentlyPlaying;

        [ObservableProperty]
        MeetingAiActionRecordResponse audioDetails;

        [ObservableProperty]
        string durationDisplay;

        CancellationTokenSource? _timerCts;

        [ObservableProperty]
        string noteScript;

        [ObservableProperty]
        ObservableCollection<AudioItem> audioSources = new();

        [ObservableProperty]
        MeetingAiActionInfoResponse meetingInfoModel;


        public IAudioRecorder recorder { get; set; }

        private DateTime? _recordStartTime;
        private TimeSpan _accumulatedDuration = TimeSpan.Zero;

        public IAudioStreamService _audioService;


        //Details
        public NotesScriptDetailsViewModel(MeetingAiActionInfoResponse item, IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioStreamService audioService)
        {
            Rep = GenericRep;
            _service = service;
            MeetingInfoModel = item;
            _audioService = audioService;

            IsScriptBtn = MeetingInfoModel.MeetingAiActionRecords.Count > 0 ? true : false;

        }


        public async Task GetMeetingInfo(string meetingAiActionId)
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.GetAsync<MeetingAiActionInfoResponse>($"{ApiConstants.GetMeetingAiActionInfoApi}{meetingAiActionId}", UserToken);
                UserDialogs.Instance.HideHud();
                if (json != null)
                {
                    MeetingInfoModel = json;
                }
            }
            IsEnable = true;
        }


        [RelayCommand]
        async Task BackButtonClicked()
        {
            //CheckAudio(AudioDetails);
            _audioService.Stop();
            await App.Current!.MainPage!.Navigation.PopAsync();
        }

        [RelayCommand]
        public async Task ToggleRecording()
        {
            if (!IsRecording)
            {
                // Start recording
                if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                {
                    await App.Current!.MainPage!.DisplayAlert(AppResources.msgWarning, AppResources.msgMicrophoneaccessisrequiredtorecordaudio, AppResources.msgOk);
                    return;
                }

                var filePath = Path.Combine(FileSystem.AppDataDirectory, $"recording_{DateTime.Now:yyyyMMddHHmmss}.wav");

                try
                {
                    // Mark start point (don’t reset _accumulatedDuration, so timer continues)
                    _recordStartTime = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(DurationDisplay))
                        DurationDisplay = "00:00:00";

                    StartDurationTimer();

                    recorder = AudioManager.Current.CreateRecorder();
                    await recorder.StartAsync(filePath);
                    IsRecording = true;
                    IsShowStopBtn = true;
                }
                catch (Exception ex)
                {
                    await App.Current!.MainPage!.DisplayAlert(AppResources.msgError, $"{AppResources.msgCouldnotstartrecording} {ex.Message}", AppResources.msgOk);
                }
            }
            else
            {
                // Pause recording
                try
                {
                    var audioSource = await recorder.StopAsync();

                    if (_recordStartTime != null)
                    {
                        // Save elapsed time into accumulator
                        _accumulatedDuration += DateTime.UtcNow - _recordStartTime.Value;
                    }

                    StopDurationTimer();
                    IsRecording = false;
                    _recordStartTime = null; // important, since paused
                }
                catch (Exception ex)
                {
                    await App.Current!.MainPage!.DisplayAlert(AppResources.msgError, $"{AppResources.msgCouldnotstoprecording} {ex.Message}", AppResources.msgOk);
                }
            }
        }

        [RelayCommand]
        async Task StopRecording()
        {
            try
            {
                var audioSource = await recorder.StopAsync();
                UserDialogs.Instance.ShowLoading();

                if (audioSource != null)
                {
                    AudioItem audio = new AudioItem { AudioSource = audioSource };

                    var rnd = new Random();
                    for (int i = 0; i < audio.Waveform.Count; i++)
                        audio.Waveform[i] = rnd.Next(5, 40);

                    IsScriptBtn = true;

                    if (_recordStartTime != null)
                    {
                        _accumulatedDuration += DateTime.UtcNow - _recordStartTime.Value;
                    }

                    audio.Duration = DurationDisplay = _accumulatedDuration.ToString(@"hh\:mm\:ss");
                    audio.RecordTime = DateTime.Now.ToString("hh:mm tt");

                    // ✅ Get bytes from IAudioSource
                    using var stream = audioSource.GetAudioStream();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();

                    AudioUploadRequest audioRequest = new AudioUploadRequest
                    {
                        AudioTime = DurationDisplay,
                        AudioData = fileBytes,
                        Extension = ".wav"
                    };


                    AudioSources.Add(audio);

                    // Reset everything
                    _recordStartTime = null;
                    _accumulatedDuration = TimeSpan.Zero;
                    DurationDisplay = string.Empty;

                    StopDurationTimer();

                    string UserToken = await _service.UserToken();
                    if (!string.IsNullOrEmpty(UserToken))
                    {
                        UserDialogs.Instance.ShowLoading();
                        var json = await Rep.PostTRAsync<AudioUploadRequest, MeetingAiActionRecordResponse>($"{ApiConstants.AddMeetingAiActionRecordApi}{MeetingInfoModel.Id}", audioRequest, UserToken);
                        UserDialogs.Instance.HideHud();
                        if (json.Item1 != null)
                        {
                            var toast = Toast.Make(AppResources.msgSuccessfullyforaddRecord, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();

                            MeetingInfoModel.MeetingAiActionRecords.Add(json.Item1);
                        }
                        else
                        {
                            var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Key + " " + json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                    }
                    IsEnable = true;

                }

                IsRecording = false;
                IsShowStopBtn = false;
                UserDialogs.Instance.HideHud();
            }
            catch (Exception ex)
            {
                await App.Current!.MainPage!.DisplayAlert(AppResources.msgError, $"{AppResources.msgCouldnotstoprecording} {ex.Message}", AppResources.msgOk);
            }
        }


        void StartDurationTimer()
        {
            _timerCts?.Cancel();
            _timerCts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_timerCts.Token.IsCancellationRequested && _recordStartTime != null)
                {
                    var elapsed = _accumulatedDuration + (DateTime.UtcNow - _recordStartTime.Value);
                    DurationDisplay = elapsed.ToString(@"hh\:mm\:ss");
                    await Task.Delay(500);
                }
            });
        }

        void StopDurationTimer()
        {
            _timerCts?.Cancel();
        }


        [RelayCommand]
        async Task PlayClicked(MeetingAiActionRecordResponse audio)
        {
            AudioDetails = audio;

            if (!AudioDetails.IsPlaying)
            {
                try
                {
                    //Device.BeginInvokeOnMainThread(async () =>
                    //{
                    //    AudioDetails = await _audioService.ReturnPlayAudio(audio);
                    //}
                    //);

                    ////AudioDetails.IsPlaying = true;
                    if (!string.IsNullOrEmpty(audio.AudioUrl))
                        await App.Current!.MainPage!.Navigation.PushAsync(new WebViewMeetingAudioPage(audio.AudioUrl));

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                _currentlyPlaying = AudioDetails;
            }
            else
            {
                //StopItem(AudioDetails);
                AudioDetails.IsPlaying = false;
            }

        }



        void StopItem(MeetingAiActionRecordResponse audio)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    AudioDetails = await _audioService.ReturnStopAudio(audio);
                }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{AppResources.msgError} {ex.Message}");
            }
        }


        //public void CheckAudio(MeetingAiActionRecordResponse audio)
        //{
        //    if (audio == null)
        //        return;

        //    // If this item is already playing → stop it
        //    if (audio.IsPlaying)
        //    {
        //        StopItem(audio);
        //        _currentlyPlaying = null;
        //        return;
        //    }

        //    // If another item is playing → stop it first
        //    if (_currentlyPlaying != null)
        //    {
        //        StopItem(_currentlyPlaying);
        //    }
        //}


        [RelayCommand]
        public async Task DeleteRecord(MeetingAiActionRecordResponse model)
        {
            bool result = await App.Current!.MainPage!.DisplayAlert(AppResources.msgDeleteRecord, AppResources.msgAreyousuredeleterecord, AppResources.msgYes, AppResources.msgNo);
            if (result)
            {
                IsEnable = false;
                string UserToken = await _service.UserToken();
                if (!string.IsNullOrEmpty(UserToken))
                {
                    UserDialogs.Instance.ShowLoading();
                    string response = await Rep.PostEAsync($"{ApiConstants.DeleteMeetingAiActionRecordApi}{model.MeetingAiActionId}/{model.Id}", UserToken);
                    UserDialogs.Instance.HideHud();
                    if (response == "")
                    {
                        UserDialogs.Instance.ShowLoading();
                        await GetMeetingInfo(model.MeetingAiActionId);
                        UserDialogs.Instance.HideHud();
                    }
                    else
                    {
                        var toast = Toast.Make(AppResources.msgTheRecordhasnotbeendeleted, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                IsEnable = true;
            }
        }


        [RelayCommand]
        public async Task CreateAnalyzeScript(MeetingAiActionInfoResponse model)
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                MeetingAiActionRecordAnalyzeRequest obj = new MeetingAiActionRecordAnalyzeRequest();
                obj.MeetingAiActionRecordIds = new List<string>();
                model.MeetingAiActionRecords.ToList().ForEach(f =>
                {
                    if (f.IsScript == true)
                    {
                        obj.MeetingAiActionRecordIds.Add(f.Id);
                    }
                });

                if (obj.MeetingAiActionRecordIds.Count > 0)
                {

                    UserDialogs.Instance.ShowLoading();
                    var json = await Rep.PostTRAsync<MeetingAiActionRecordAnalyzeRequest, MeetingAiActionRecordAnalyzeResponse>($"{ApiConstants.AddMeetingAiActionRecordAnalyzeApi}{model.Id}", obj, UserToken);
                    UserDialogs.Instance.HideHud();
                    if (json.Item1 != null)
                    {
                        var toast = Toast.Make(AppResources.msgSuccessfullyforcreatemeetingscript, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();

                        await GetMeetingInfo(model.Id);
                    }
                    else
                    {
                        var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Key + " " + json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                else
                {
                    var toast = Toast.Make(AppResources.msgRequiredminimumselectonerecord, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }
            IsEnable = true;
        }

        [RelayCommand]
        public async Task FullScreenRecordNote(MeetingAiActionRecordResponse model)
        {
            IsEnable = false;
            await App.Current!.MainPage!.Navigation.PushAsync(new FullScreenRecordNotePage(this, model.AudioScript));
            IsEnable = true;
        }

        [RelayCommand]
        public async Task FullScreenScript(MeetingAiActionInfoResponse model)
        {
            IsEnable = false;
            await App.Current!.MainPage!.Navigation.PushAsync(new FullScreenScriptPage(this, model.MeetingAiActionRecordAnalyzeResponse?.AnalyzeScript));
            IsEnable = true;
        }

        //private async Task<string> ConvertSpeechToTextAsync()
        //{
        //    try
        //    {
        //        // Start speech recognition
        //        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        //        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        //        var result = await recognizer.RecognizeOnceAsync();

        //        if (result.Reason == ResultReason.RecognizedSpeech)
        //        {
        //            return result.Text;
        //        }
        //        else if (result.Reason == ResultReason.NoMatch)
        //        {
        //            return $"{AppResources.msgSpeechnotrecognizedPleasetryagain}";
        //        }
        //        else if (result.Reason == ResultReason.Canceled)
        //        {
        //            var cancellation = CancellationDetails.FromResult(result);
        //            return $"CANCELED: Reason={cancellation.Reason}, ErrorCode={cancellation.ErrorCode}, ErrorDetails={cancellation.ErrorDetails}";
        //        }
        //        else
        //        {
        //            return $"{AppResources.msgSpeechrecognitionfailed} {result.Reason}";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"{AppResources.msgErrorduringspeechrecognition} {ex.Message}";
        //    }
        //}



    }
}

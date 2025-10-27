
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models.MeetingAiAction;
using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.Models.MeetingAiActionRecordAnalyze;
using Cardrly.Pages.MeetingsScript;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using GoogleApi.Entities.Translate.Common.Enums;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Mopups.Services;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.Text;



namespace Cardrly.ViewModels.MeetingsAi
{
    public partial class NotesScriptDetailsViewModel : BaseViewModel
    {

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        [ObservableProperty]
        bool isScriptBtn;


        [ObservableProperty]
        MeetingAiActionRecordResponse audioDetails;

        MeetingAiActionRecordResponse _currentlyPlaying;


        [ObservableProperty]
        bool isShowExpander = false;

        [ObservableProperty]
        bool isShowGetScript = false;


        [ObservableProperty]
        MeetingAiActionInfoResponse meetingInfoModel;


        public IAudioStreamService _audioService;

        MeetingAiActionResponse MeetingAiAction = new();


        //Details
        public NotesScriptDetailsViewModel(MeetingAiActionResponse model, IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioStreamService audioService)
        {
            Rep = GenericRep;
            _service = service;
            _audioService = audioService;
            MeetingAiAction = model;
            Init(MeetingAiAction.Id);

            //AddOrDeleteRecord
            MessagingCenter.Subscribe<RecordViewModel, string>(this, "AddOrDeleteRecord", async (sender, message) =>
            {
                if (true)
                {
                    await GetMeetingInfo(message);
                }
            });

            //AddOrDeleteRecord
            MessagingCenter.Subscribe<NotesScriptDetailsViewModel, string>(this, "AddOrDeleteRecord", async (sender, message) =>
            {
                if (true)
                {
                    await GetMeetingInfo(message);
                }
            });

        }

        async void Init(string meetingAiActionId)
        {
            await GetMeetingInfo(meetingAiActionId);

            IsScriptBtn = MeetingInfoModel.MeetingAiActionRecords.Count > 0 ? true : false;

            if (!string.IsNullOrEmpty(MeetingInfoModel.MeetingAiActionRecordAnalyzeResponse?.AnalyzeScript) || !string.IsNullOrEmpty(MeetingInfoModel.MeetingAiActionRecordAnalyzeResponse?.AudioAllScript))
            {
                IsShowExpander = true;
            }    
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
                    Controls.StaticMember.AzureMeetingAiSekrtKey = json.SecretKey ?? "";
                    MeetingInfoModel = json;
                }
            }
            IsEnable = true;
        }


        [RelayCommand]
        async Task BackButtonInfoClicked()
        {
            await App.Current!.MainPage!.Navigation.PopAsync();
        }


        [RelayCommand]
        async Task PlayClicked(MeetingAiActionRecordResponse audio)
        {
            AudioDetails = audio;

            if (!AudioDetails.IsPlaying)
            {
                try
                {
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
                AudioDetails.IsPlaying = false;
            }

        }


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
                        //await GetMeetingInfo(model.MeetingAiActionId);
                        MessagingCenter.Send(this, "AddOrDeleteRecord", model.MeetingAiActionId);
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
        public async Task FullScreenAnalyzeScript(MeetingAiActionInfoResponse model)
        {
            IsEnable = false;
            await App.Current!.MainPage!.Navigation.PushAsync(new FullScreenScriptPage(this, model.MeetingAiActionRecordAnalyzeResponse, 1));//1 = AnalyzeScript
            IsEnable = true;
        }

        [RelayCommand]
        public async Task FullScreenAudioAllScript(MeetingAiActionInfoResponse model)
        {
            IsEnable = false;
            await App.Current!.MainPage!.Navigation.PushAsync(new FullScreenScriptPage(this, model.MeetingAiActionRecordAnalyzeResponse, 2));//2 = AudioAllScript
            IsEnable = true;
        }

        [RelayCommand]
        public async Task GoToSettingPopupBeforeRecordPage()
        {
            IsEnable = false;
            await MopupService.Instance.PushAsync(new MeetingSettingPopup(new MeetingSettingViewModel(MeetingInfoModel,Rep,_service,_audioService)));
            IsEnable = true;
        }

       


        [RelayCommand]
        public async Task GetPDF(string scriptText)
        {
            IsEnable = false;
            string UserToken = await _service.UserToken();
            if (!string.IsNullOrEmpty(UserToken))
            {
                GeneratePdfRequest oGeneratePdfRequest = new GeneratePdfRequest
                {
                    Text = scriptText,
                };
                UserDialogs.Instance.ShowLoading();
                var json = await Rep.PostTRAsync<GeneratePdfRequest, PdfResponse>(ApiConstants.PostMeetingAiScriptTextToPDFApi, oGeneratePdfRequest, UserToken);
                UserDialogs.Instance.HideHud();
                if (json.Item1 != null)
                {
                    if (json.Item1 != null && json.Item1.Success && !string.IsNullOrEmpty(json.Item1.PdfBytes))
                    {
                        // Convert Base64 string to bytes
                        byte[] pdfBytes = Convert.FromBase64String(json.Item1.PdfBytes);

                        // Use filename from server or default
                        string fileName = string.IsNullOrEmpty(json.Item1.FileName)
                            ? "Meeting.pdf"
                            : json.Item1.FileName;

                        await SaveAndOpenPdfAsync(pdfBytes, fileName);
                    }
                    else
                    {
                        await App.Current!.MainPage!.DisplayAlert(AppResources.msgError, AppResources.msgFailedtoparsePDFresponse, AppResources.msgOk);
                    }
                }
                else
                {
                    var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Key + " " + json.Item2?.errors?.FirstOrDefault().Value}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }
            IsEnable = true;
        }


        public async Task SaveAndOpenPdfAsync(byte[] pdfBytes, string fileName = "Meeting.pdf")
        {
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            File.WriteAllBytes(filePath, pdfBytes);
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
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

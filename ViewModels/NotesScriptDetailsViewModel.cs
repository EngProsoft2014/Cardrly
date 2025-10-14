
using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
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



namespace Cardrly.ViewModels
{
    public partial class NotesScriptDetailsViewModel : BaseViewModel
    {

        [ObservableProperty]
        ObservableCollection<MeetingMessage> messages = new();
        private readonly Dictionary<string, Color> _speakerColors = new();
        private readonly Random _random = new();

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
        ObservableCollection<ScriptTypeModel> scriptTypes = new();
        [ObservableProperty]
        ScriptTypeModel selectedScriptType = new();

        MeetingAiActionRecordResponse? _currentlyPlaying;

        [ObservableProperty]
        string selectedLanguage = string.Empty;

        [ObservableProperty]
        bool isEnableLang = true;

        [ObservableProperty]
        bool isEnableScriptType = true;

        [ObservableProperty]
        MeetingAiActionRecordResponse audioDetails;

        [ObservableProperty]
        string durationDisplay;

        CancellationTokenSource? _timerCts;

        [ObservableProperty]
        string noteScript;

        [ObservableProperty]
        bool isShowExpander = false;

        [ObservableProperty]
        bool isShowGetScript = false;

        [ObservableProperty]
        ObservableCollection<AudioItem> audioSources = new();

        [ObservableProperty]
        MeetingAiActionInfoResponse meetingInfoModel;

        private StringBuilder _transcriptBuilder = new();


        public IAudioRecorder recorder { get; set; }

        public DateTime? _recordStartTime;
        public TimeSpan _accumulatedDuration = TimeSpan.Zero;

        public IAudioStreamService _audioService;

        private readonly List<string> recordedParts = new();

        //Test Speech to Text
        SpeechRecognizer _speechRecognizer;
        SpeechConfig speechConfig;

        private string _partialText = string.Empty; // stores current live phrase

        // Track live partials by speaker
        private readonly Dictionary<string, string> _partialTexts = new();

        private readonly Dictionary<string, MeetingMessage> _liveMessages = new();

        private ConversationTranscriber? _conversationTranscriber;
        private bool _isTranscribing = false;

        AutoDetectSourceLanguageConfig autoLangConfig;

        string speechKey;
        string speechRegion;

        //Details
        public NotesScriptDetailsViewModel(MeetingAiActionResponse model, IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioStreamService audioService)
        {
            Rep = GenericRep;
            _service = service;
            _audioService = audioService;

            Init(model.Id);

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

            ScriptTypes.Add(new ScriptTypeModel { Id = 1, Name = "Simple Script" });
            ScriptTypes.Add(new ScriptTypeModel { Id = 2, Name = "Meeting Script" });

            IsScriptBtn = MeetingInfoModel.MeetingAiActionRecords.Count > 0 ? true : false;

            if (!string.IsNullOrEmpty(MeetingInfoModel.MeetingAiActionRecordAnalyzeResponse?.AnalyzeScript) || !string.IsNullOrEmpty(MeetingInfoModel.MeetingAiActionRecordAnalyzeResponse?.AudioAllScript))
            {
                IsShowExpander = true;
            }

            // Initialize Azure Speech Service client
            speechKey = Controls.StaticMember.AzureMeetingAiSekrtKey;
            speechRegion = "eastus"; //"YOUR_REGION"; // Example: "eastus"

            if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                await App.Current!.MainPage!.DisplayAlert(
                    "Speech Disabled",
                    "Speech recognition is not supported on iOS simulator. Please test on a real device.",
                    "OK");
                return;
            }

            //Test Speech to Text
            if (string.IsNullOrEmpty(speechKey))
            {
                return;
            }
            else
            {
                speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);

                speechConfig.SetProperty("ConversationTranscriptionInRealTime", "true");
                speechConfig.SetProperty("SpeechServiceResponse_PostProcessingOption", "TrueText"); // optional
                speechConfig.SetProperty("ConversationTranscriptionInRealTime", "true");
                speechConfig.SetProperty("SpeechServiceResponse_EnablePartialResultStabilization", "true");
                speechConfig.SetProperty("SpeechServiceResponse_StablePartialResultThreshold", "2");

                speechConfig.SpeechRecognitionLanguage = "ar-EG";//Default Language
            }

            autoLangConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[]
            {
                "en-US", // English
                "ar-EG", // Arabic (Egypt)
            });
        }

        // Assign consistent color per speaker
        private void AddTranscript(string speaker, string text)
        {
            if (!_speakerColors.ContainsKey(speaker))
            {
                var colors = new[] { Colors.OrangeRed, Colors.Black, Colors.Blue, Colors.DarkBlue, Colors.Green, Colors.Brown, Colors.Purple };
                _speakerColors[speaker] = colors[_random.Next(colors.Length)];
            }

            Messages.Add(new MeetingMessage
            {
                Speaker = speaker,
                Text = text,
                TextColor = _speakerColors[speaker]
            });
        }

        private bool IsTextValidLanguage(string text)
        {
            foreach (var c in text)
            {
                // Arabic letters & symbols
                bool isArabic =
                    (c >= 0x0600 && c <= 0x06FF) ||
                    (c >= 0x0750 && c <= 0x077F) ||
                    (c >= 0x08A0 && c <= 0x08FF);

                // English letters
                bool isEnglish = char.IsLetter(c) && c <= 0x007A; // a-zA-Z plus basic Latin letters

                // Digits
                bool isDigit = char.IsDigit(c);

                // Whitespace or common punctuation
                bool isAllowedPunctuation = char.IsWhiteSpace(c) || ".,!?':;-()\"".Contains(c) || "،؛؟".Contains(c);

                if (!(isArabic || isEnglish || isDigit || isAllowedPunctuation))
                    return false;
            }
            return true;
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
        async Task BackButtonClicked()
        {
            if (Messages.Count > 0 || !string.IsNullOrEmpty(NoteScript))
            {
                bool Pass = await App.Current!.MainPage!.DisplayAlert(AppResources.Info, AppResources.msgDoyouwanttosavetherecording, AppResources.msgOk, AppResources.btnCancel);

                if (Pass)
                {
                    _audioService.Stop();
                    await StopRecording();
                    await App.Current!.MainPage!.Navigation.PopAsync();
                }
                else
                {
                    // Reset everything
                    await ResetUi();
                }
            }
            else
            {
                // Reset everything
                await ResetUi();
                _accumulatedDuration = TimeSpan.Zero;
            }
        }

        [RelayCommand]
        async Task BackButtonInfoClicked()
        {
            await App.Current!.MainPage!.Navigation.PopAsync();
        }

        public async Task ResetUi()
        {
            // Reset everything
            if (recorder != null)
            {
                await recorder.StopAsync();
                StopDurationTimer();
                IsRecording = false;

                if (_recordStartTime != null)
                {
                    // Save elapsed time into accumulator
                    _accumulatedDuration += DateTime.UtcNow - _recordStartTime.Value;
                }

                if (SelectedScriptType?.Id == 1) //Simple Script
                {
                    await _speechRecognizer.StopContinuousRecognitionAsync();
                }
                else if (SelectedScriptType?.Id == 2) //Meeting Script
                {
                    if (_conversationTranscriber != null && _isTranscribing)
                    {
                        await _conversationTranscriber.StopTranscribingAsync();
                        _isTranscribing = false;
                    }
                }
            }

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

                // 🎤 START or RESUME RECORDING
                IsRecording = true;
                IsShowStopBtn = true;
                IsEnable = false;
                IsEnableLang = false;
                IsEnableScriptType = false;

                var filePath = Path.Combine(FileSystem.AppDataDirectory, $"recording_{DateTime.Now:yyyyMMddHHmmss}.wav");

                try
                {

                    if (SelectedLanguage == "English")
                        speechConfig.SpeechRecognitionLanguage = "en-US";
                    else if (SelectedLanguage == "العربية")
                        speechConfig.SpeechRecognitionLanguage = "ar-EG";


                    recorder = AudioManager.Current.CreateRecorder();
                    await recorder.StartAsync(filePath);

                    // Save path for merging later
                    recordedParts.Add(filePath);

                    // Mark start point (don’t reset _accumulatedDuration, so timer continues)
                    _recordStartTime = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(DurationDisplay))
                        DurationDisplay = "00:00:00";

                    StartDurationTimer();

                    if (SelectedScriptType.Id == 1) //Simple Script
                    {
                        if (_speechRecognizer == null)
                        {
                            _speechRecognizer = new SpeechRecognizer(speechConfig, autoLangConfig, AudioConfig.FromDefaultMicrophoneInput());

                            _speechRecognizer.Recognizing += async (s, e) =>
                            {
                                // keep live partial UI
                                _partialText = e.Result.Text?.Trim() ?? string.Empty;
                                NoteScript = $"{_transcriptBuilder}{(_partialText.Length > 0 ? " " + _partialText : string.Empty)}";
                            };

                            // 🔹 FINAL event - recognized (stable result)
                            _speechRecognizer.Recognized += (s, e) =>
                            {
                                if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
                                {
                                    _transcriptBuilder.AppendLine(e.Result.Text.Trim());
                                    _partialText = string.Empty;
                                    NoteScript = _transcriptBuilder.ToString();
                                }
                            };

                            _speechRecognizer.Canceled += (s, e) => { /* optional logging */ };
                            _speechRecognizer.SessionStopped += (s, e) => { /* optional logging */ };
                        }
                        await _speechRecognizer.StartContinuousRecognitionAsync();
                    }
                    else if (SelectedScriptType.Id == 2) //Meeting Script
                    {
                        _conversationTranscriber = new ConversationTranscriber(speechConfig, AudioConfig.FromDefaultMicrophoneInput());

                        // 🔹 Event: partial recognized speech
                        _conversationTranscriber.Transcribing += (s, e) =>
                        {
                            var speaker = e.Result.SpeakerId ?? AppResources.lblScript;
                            var partial = e.Result.Text?.Trim() ?? string.Empty;

                            if (string.IsNullOrEmpty(partial))
                                return;

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (!_liveMessages.ContainsKey(speaker))
                                {
                                    // Create a temporary live message
                                    var msg = new MeetingMessage
                                    {
                                        Speaker = speaker,
                                        Text = partial + "...",
                                        TextColor = _speakerColors.ContainsKey(speaker)
                                            ? _speakerColors[speaker]
                                            : Colors.Red
                                    };
                                    _liveMessages[speaker] = msg;
                                    Messages.Add(msg);
                                }
                                else
                                {
                                    _liveMessages[speaker].Text = partial + "...";
                                }
                            });
                        };

                        // 🔹 Event: finalized text (speaker + content)
                        _conversationTranscriber.Transcribed += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
                            {
                                var speaker = e.Result.SpeakerId ?? AppResources.lblScript;
                                var text = e.Result.Text.Trim();

                                MainThread.BeginInvokeOnMainThread(async () =>
                                {
                                    if (!IsTextValidLanguage(text))
                                    {
                                        var toast = Toast.Make(AppResources.msgUnsupportLang, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                                        await toast.Show();
                                    }

                                    if (_liveMessages.ContainsKey(speaker))
                                    {
                                        // Update the existing message to final text
                                        _liveMessages[speaker].Text = text;
                                        _liveMessages.Remove(speaker);
                                    }
                                    else
                                    {
                                        AddTranscript(speaker, text);
                                    }
                                });
                            }
                        };

                        _conversationTranscriber.Canceled += (s, e) =>
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await App.Current!.MainPage!.DisplayAlert("Error",
                                    $"Recognition failed. Reason: {e.Reason}\nDetails: {e.ErrorDetails}",
                                    "OK");
                            });
                        };

                        _conversationTranscriber.SessionStopped += (s, e) =>
                        {
                            // Optional logging or UI cleanup
                        };

                        await _conversationTranscriber.StartTranscribingAsync();
                        _isTranscribing = true;
                    }

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
                    // ⏸️ PAUSE RECORDING
                    IsRecording = false;

                    var audioSource = await recorder.StopAsync();

                    recorder = null;

                    if (_recordStartTime != null)
                    {
                        // Save elapsed time into accumulator
                        _accumulatedDuration += DateTime.UtcNow - _recordStartTime.Value;
                        _recordStartTime = null;
                    }

                    StopDurationTimer();

                    _recordStartTime = null; // important, since paused

                    IsEnableLang = true;

                    if (SelectedScriptType.Id == 1) //Simple Script
                    {
                        await _speechRecognizer.StopContinuousRecognitionAsync();
                    }
                    else if (SelectedScriptType.Id == 2) //Meeting Script
                    {
                        if (_conversationTranscriber != null && _isTranscribing)
                        {
                            await _conversationTranscriber.StopTranscribingAsync();
                            _isTranscribing = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await App.Current!.MainPage!.DisplayAlert(AppResources.msgError, $"{AppResources.msgCouldnotstoprecording} {ex.Message}", AppResources.msgOk);
                }
            }
        }


        [RelayCommand]
        public async Task StopRecording()
        {
            try
            {
                if (recorder != null)
                {
                    await recorder.StopAsync();
                    recorder = null;
                }

                UserDialogs.Instance.ShowLoading();

                if (recordedParts.Count > 0)
                {
                    await Task.Delay(500); // 🕒 allow OS flush

                    AudioItem audio = new AudioItem();

                    if (_recordStartTime != null)
                        _accumulatedDuration += DateTime.UtcNow - _recordStartTime.Value;

                    audio.Duration = DurationDisplay = _accumulatedDuration.ToString(@"hh\:mm\:ss");
                    audio.RecordTime = DateTime.Now.ToString("hh:mm tt");

                    // ✅ Safe merging directly to file (not MemoryStream)
                    var mergedFilePath = Path.Combine(FileSystem.AppDataDirectory, $"merged_{DateTime.Now:yyyyMMddHHmmss}.wav");

                    using (var output = File.Create(mergedFilePath))
                    {
                        bool isFirst = true;
                        long totalDataLength = 0;

                        foreach (var partPath in recordedParts)
                        {
                            if (!File.Exists(partPath))
                                continue;

                            var bytes = await File.ReadAllBytesAsync(partPath);
                            if (bytes.Length < 100) // skip empty/invalid
                                continue;

                            if (isFirst)
                            {
                                await output.WriteAsync(bytes, 0, bytes.Length);
                                totalDataLength += bytes.Length - 44;
                                isFirst = false;
                            }
                            else
                            {
                                await output.WriteAsync(bytes, 44, bytes.Length - 44);
                                totalDataLength += bytes.Length - 44;
                            }

                            // 🛑 optional limit safeguard
                            if (output.Length > 3_500_000_000)
                                break;
                        }

                        // ✅ Fix header after full merge
                        output.Seek(4, SeekOrigin.Begin);
                        await output.WriteAsync(BitConverter.GetBytes((int)(totalDataLength + 36)));
                        output.Seek(40, SeekOrigin.Begin);
                        await output.WriteAsync(BitConverter.GetBytes((int)totalDataLength));
                    }

                    // 🔹 Convert merged file to bytes
                    var mergedBytes = await File.ReadAllBytesAsync(mergedFilePath);

                    // Prepare upload request
                    AudioUploadRequest audioRequest = new AudioUploadRequest
                    {
                        AudioTime = DurationDisplay,
                        AudioData = mergedBytes,
                        AudioScript = NoteScript,
                        LstMeetingMessage = Messages.ToList(),
                        Extension = ".wav"
                    };

                    AudioSources.Add(audio);

                    // 🔹 Reset state
                    _recordStartTime = null;
                    _accumulatedDuration = TimeSpan.Zero;
                    DurationDisplay = string.Empty;
                    StopDurationTimer();

                    // 🔹 Upload
                    string userToken = await _service.UserToken();
                    if (!string.IsNullOrEmpty(userToken))
                    {
                        var json = await Rep.PostTRAsync<AudioUploadRequest, MeetingAiActionRecordResponse>(
                            $"{ApiConstants.AddMeetingAiActionRecordApi}{MeetingInfoModel.Id}",
                            audioRequest,
                            userToken);

                        if (json.Item1 != null)
                        {
                            var toast = Toast.Make(AppResources.msgSuccessfullyforaddRecord, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();

                            MeetingInfoModel.MeetingAiActionRecords.Add(json.Item1);
                            MessagingCenter.Send(this, "AddOrDeleteRecord", json.Item1.MeetingAiActionId);
                            await App.Current!.MainPage!.Navigation.PopAsync();
                        }
                        else
                        {
                            var toast = Toast.Make($"{json.Item2?.errors?.FirstOrDefault().Key} {json.Item2?.errors?.FirstOrDefault().Value}",
                                CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();
                        }
                    }

                    // 🧹 Cleanup temporary files
                    try
                    {
                        foreach (var path in recordedParts)
                        {
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                        recordedParts.Clear();
                    }
                    catch { }

                    IsEnable = true;
                }

                IsRecording = false;
                IsShowStopBtn = false;
            }
            catch (Exception ex)
            {
                await App.Current!.MainPage!.DisplayAlert(AppResources.msgError, $"{AppResources.msgCouldnotstoprecording} {ex.Message}", AppResources.msgOk);
            }
            finally
            {
                UserDialogs.Instance.HideHud();
            }
        }



        public void StartDurationTimer()
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

        public void StopDurationTimer()
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
            await MopupService.Instance.PushAsync(new MeetingSettingPopup(this));
            IsEnable = true;
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
                await App.Current!.MainPage!.Navigation.PushAsync(new RecordPage(this));

                await MopupService.Instance.PopAsync();

                await ToggleRecording();
            }

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

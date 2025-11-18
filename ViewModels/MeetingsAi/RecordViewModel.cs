using Cardrly.Constants;
using Cardrly.Helpers;
using Cardrly.Models;
using Cardrly.Models.MeetingAiAction;
using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Newtonsoft.Json;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.Text;
using System.ComponentModel;
using Cardrly.Pages.MeetingsScript;
using Akavache;
using System.Reactive.Linq;
using GoogleApi.Entities.Maps.StaticMaps.Request;
using Cardrly.Services.NativeAudioRecorder;



#if ANDROID
using Android.Content;
#endif

namespace Cardrly.ViewModels.MeetingsAi
{
    public partial class RecordViewModel : BaseViewModel
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
        bool isShowStopBtn;


        [ObservableProperty]
        ScriptTypeModel selectedScriptType = new();

        [ObservableProperty]
        string selectedLanguage = string.Empty;

        [ObservableProperty]
        MeetingAiActionRecordResponse audioDetails;

        [ObservableProperty]
        string durationDisplay = "00:00:00";

        [ObservableProperty]
        string startDurationDisplay = "00:00:00";

        [ObservableProperty]
        string bradgeDurationDisplay = "00:00:00";

        CancellationTokenSource? _timerCts;

        IDispatcherTimer? _timer;

        [ObservableProperty]
        string noteScript;

        [ObservableProperty]
        bool isShowExpander = false;

        [ObservableProperty]
        bool isShowGetScript = false;

        [ObservableProperty]
        MeetingAiActionInfoResponse meetingInfoModel;

        private StringBuilder _transcriptBuilder = new();

        byte[]? uploadBytes;

#if IOS
        private Cardrly.Services.NativeAudioRecorder.INativeAudioRecorder recorder;
#endif

#if ANDROID
        private Platforms.Android.Receivers.AudioFocusChangeListener? _focusListener;
#endif

        public DateTime? _recordStartTime;
        public TimeSpan _accumulatedDuration = TimeSpan.Zero;

        public IAudioStreamService _audioService;

        private readonly List<string> recordedParts = new();

        //Test Speech to Text
        SpeechRecognizer _speechRecognizer;
        SpeechConfig speechConfig;

        private string _partialText = string.Empty; // stores current live phrase

        private readonly Dictionary<string, MeetingMessage> _liveMessages = new();

        private ConversationTranscriber? _conversationTranscriber;
        private bool _isTranscribing = false;

        AutoDetectSourceLanguageConfig autoLangConfig;

        private bool _isRecognizerRunning = false;

        private DateTime _lastRestartTime = DateTime.UtcNow;

        string speechKey;
        string speechRegion;

        private readonly TimeSpan MaxRecordingDuration = TimeSpan.FromHours(1);
        private bool _alert10MinShown = false;
        private bool _alert1MinShown = false;
        private bool _recordingStoppedByTime = false;

        public RecordViewModel(MeetingAiActionInfoResponse model, ScriptTypeModel scriptType, string selecteLang, IGenericRepository GenericRep, Services.Data.ServicesService service, IAudioStreamService audioService)
        {
            MeetingInfoModel = model;
            SelectedScriptType = scriptType;
            SelectedLanguage = selecteLang;
            Rep = GenericRep;
            _service = service;
            _audioService = audioService;
            Init();
        }

        async void Init()
        {
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
                speechConfig.SetProperty("SPEECH-AudioInputStreamKeepAlive", "true");
                speechConfig.SetProperty("SPEECH-AudioBufferSizeInSeconds", "20"); // default is 10

                speechConfig.SpeechRecognitionLanguage = "ar-EG";//Default Language
            }

            autoLangConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[]
            {
                "en-US", // English
                "ar-EG", // Arabic (Egypt)
            });

            //After Init Azure Speech Service client
            await ToggleRecording();
        }


        // Assign consistent color per speaker
        private void AddTranscript(string speaker, string text)
        {
            if (!_speakerColors.ContainsKey(speaker))
            {
                var colors = new[] { Colors.OrangeRed, Colors.Black, Colors.Blue, Colors.DarkBlue, Colors.Green, Colors.Brown, Colors.Purple };
                _speakerColors[speaker] = colors[_random.Next(colors.Length)];
            }

            StartDurationDisplay = BradgeDurationDisplay;

            Messages.Add(new MeetingMessage
            {
                Speaker = speaker,
                SpeakerDuration = StartDurationDisplay,
                Text = text,
                TextColor = _speakerColors[speaker]
            });

            BradgeDurationDisplay = DurationDisplay;
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

        [RelayCommand]
        async Task BackButtonClicked()
        {
            if (Messages.Count > 0 || !string.IsNullOrEmpty(NoteScript))
            {
                //bool Pass = await App.Current!.MainPage!.DisplayAlert(AppResources.Info, AppResources.msgDoyouwanttosavetherecording, AppResources.msgOk, AppResources.btnCancel);

                //if (Pass)
                //{
                //    _audioService.Stop();
                //    await StopRecording();
                //    await App.Current!.MainPage!.Navigation.PopAsync();
                //}
                //else
                //{
                //    // Reset everything
                //    await ResetUi();
                //}

                await Mopups.Services.MopupService.Instance.PushAsync(new ConfirmRecordPopup(this));
            }
            else
            {
                // Reset everything
                await ResetUi();
            }
        }

        public async Task ResetUi()
        {

            StopDurationTimer();
            IsRecording = false;

            _alert10MinShown = false;
            _alert1MinShown = false;
            _recordingStoppedByTime = false;

            if (string.IsNullOrEmpty(DurationDisplay))
                DurationDisplay = "00:00:00";

            if (_recordStartTime != null)
            {
                _accumulatedDuration = TimeSpan.Zero;
            }
            Messages.Clear();
            NoteScript = string.Empty;

#if IOS
            if (recorder != null)
            {
                await recorder.Stop();
                await Task.Delay(300); // ✅ Let iOS finalize the file
                recorder = null;
            }
#elif ANDROID
            await AndroidAudioRecorder.StopAsync();
            StopForegroundRecordingService();
#endif

            if (SelectedScriptType?.Id == 1) //Simple Script
            {
                await _speechRecognizer.StopContinuousRecognitionAsync();
                //_speechRecognizer.Recognizing -= null;
                //_speechRecognizer.Recognized -= null;
            }
            else if (SelectedScriptType?.Id == 2) //Meeting Script
            {
                if (_conversationTranscriber != null && _isTranscribing)
                {
                    await _conversationTranscriber.StopTranscribingAsync();
                    //_conversationTranscriber.Transcribing -= null;
                    //_conversationTranscriber.Transcribed -= null;
                    _isTranscribing = false;
                }
            }

            await App.Current!.MainPage!.Navigation.PopAsync();
        }

        public async Task ResetRecord()
        {
            StopDurationTimer();

            _alert10MinShown = false;
            _alert1MinShown = false;
            _recordingStoppedByTime = false;
#if IOS
            if (recorder != null)
            {
                await recorder.Stop();
                recorder = null;
            }        
#elif ANDROID
            await AndroidAudioRecorder.StopAsync();
            StopForegroundRecordingService();
#endif

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

        

        [RelayCommand]
        public async Task StopRecording()
        {
            try
            {
                await ResetRecord();

                // 🔹 Show Uploading dialog (spinner with progress text)
                UserDialogs.Instance.Loading("Uploading... 0%", maskType: MaskType.Clear);

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

#if ANDROID
                    using (var output = File.Create(mergedFilePath))
                    {
                        bool isFirst = true;

                        foreach (var partPath in recordedParts)
                        {
                            if (!File.Exists(partPath))
                                continue;

                            using (var input = File.OpenRead(partPath))
                            {
                                if (isFirst)
                                {
                                    await input.CopyToAsync(output); // include header
                                    isFirst = false;
                                }
                                else
                                {
                                    input.Seek(44, SeekOrigin.Begin); // skip header
                                    await input.CopyToAsync(output);
                                }
                            }
                        }

                        // ✅ Fix WAV header
                        var totalDataLength = output.Length - 44;
                        output.Seek(4, SeekOrigin.Begin);
                        await output.WriteAsync(BitConverter.GetBytes((int)(totalDataLength + 36)));
                        output.Seek(40, SeekOrigin.Begin);
                        await output.WriteAsync(BitConverter.GetBytes((int)totalDataLength));
                    }
#elif IOS
                    await MergeWavFilesFixedAsync(mergedFilePath, recordedParts);
#endif
                    //must be before this line File.Delete(mergedFilePath)
                    // Step 1: Read WAV bytes for backup
                    var backupBytes = File.ReadAllBytes(mergedFilePath);


                    // 🔹 Upload
                    string userToken = await _service.UserToken();
                    if (!string.IsNullOrEmpty(userToken))
                    {
                        //var parts = await SplitLargeFileAsync(mergedFilePath, 25 * 1024 * 1024); // split by 25 MB
                        
                        AudioUploadRequest audioRequest = new AudioUploadRequest
                        {
                            AudioUploadId = Guid.NewGuid().ToString(),
                            AudioTime = DurationDisplay,
                            AudioPath = mergedFilePath,
                            AudioScript = NoteScript,
                            LstMeetingMessage = Messages.Select(f => new MeetingMessage
                            {
                                Speaker = f.Speaker + "  " + f.SpeakerDuration,
                                Text = f.Text,
                            }).ToList(),
                            Extension = ".mp3",
                            AudioBytes = backupBytes,
                        };

                        // Save to Akavache
                        await BlobCache.LocalMachine.InsertObject($"upload_{MeetingInfoModel.Id}_{audioRequest.AudioUploadId}", audioRequest);

                        // 🔹 Reset state
                        _recordStartTime = null;
                        _accumulatedDuration = TimeSpan.Zero;
                        DurationDisplay = string.Empty;

                        var json = await Rep.PostFileWithFormAsync<MeetingAiActionRecordResponse>(
                        $"{ApiConstants.AddMeetingAiActionRecordApi}{MeetingInfoModel?.Id}",
                        audioRequest, userToken);

                        // 🔹 Show final completion (100%)
                        //UserDialogs.Instance.Loading("Uploading... 100%", null, true, MaskType.Clear, null);
                        //await Task.Delay(500);
                        //UserDialogs.Instance.HideHud();

                        if (json.Item1 != null)
                        {
                            var toast = Toast.Make(AppResources.msgSuccessfullyforaddRecord, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                            await toast.Show();

                            MeetingInfoModel.MeetingAiActionRecords.Add(json.Item1);
                            MessagingCenter.Send(this, "AddOrDeleteRecord", json.Item1.MeetingAiActionId);

                            await BlobCache.LocalMachine.InvalidateObject<AudioUploadRequest>($"upload_{MeetingInfoModel.Id}_{audioRequest.AudioUploadId}");
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

        public static async Task MergeWavFilesFixedAsync(string outputPath, List<string> inputPaths)
        {
            if (inputPaths == null || inputPaths.Count == 0)
                throw new InvalidOperationException("No input files to merge.");

            const int sampleRate = 16000;
            const short bitsPerSample = 16;
            const short channels = 1;
            const short blockAlign = (short)(channels * (bitsPerSample / 8));
            const int byteRate = sampleRate * blockAlign;

            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            // Reserve header space (44 bytes)
            byte[] emptyHeader = new byte[44];
            await output.WriteAsync(emptyHeader);

            long totalDataLength = 0;
            bool isFirst = true;

            foreach (var path in inputPaths)
            {
                if (!File.Exists(path))
                    continue;

                using var input = File.OpenRead(path);
                if (input.Length <= 44)
                    continue;

                if (isFirst)
                {
                    await input.CopyToAsync(output);
                    totalDataLength += (input.Length - 44);
                    isFirst = false;
                }
                else
                {
                    input.Seek(44, SeekOrigin.Begin);
                    await input.CopyToAsync(output);
                    totalDataLength += (input.Length - 44);
                }
            }

            // Build correct WAV header
            int fileSize = (int)(totalDataLength + 44 - 8);

            using var headerStream = new MemoryStream();
            using var writer = new BinaryWriter(headerStream);

            // RIFF header
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt  sub-chunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);                  // Subchunk1Size (PCM)
            writer.Write((short)1);            // AudioFormat = PCM
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            // data sub-chunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write((int)totalDataLength);

            writer.Flush();

            // Write header to file start
            headerStream.Seek(0, SeekOrigin.Begin);
            output.Seek(0, SeekOrigin.Begin);
            await headerStream.CopyToAsync(output);

            await output.FlushAsync();
        }

        public void StartDurationTimer()
        {
            StopDurationTimer(); // clean any old one

            _timer = Application.Current!.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += async (s, e) =>
            {
                if (_recordStartTime == null) return;
                var elapsed = _accumulatedDuration + (DateTime.UtcNow - _recordStartTime.Value);
                DurationDisplay = elapsed.ToString(@"hh\:mm\:ss");

                var remaining = MaxRecordingDuration - elapsed;

                // 10 minutes left
                if (remaining <= TimeSpan.FromMinutes(10) && !_alert10MinShown)
                {
                    _alert10MinShown = true;
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        PlaySystemSound.PlaySound();
                    });
                }

                // 1 minute left
                if (remaining <= TimeSpan.FromMinutes(1) && !_alert1MinShown)
                {
                    _alert1MinShown = true;
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        PlaySystemSound.PlaySound();
                    });
                }

                // Auto-stop at 1 hour
                if (elapsed >= MaxRecordingDuration && !_recordingStoppedByTime)
                {
                    _recordingStoppedByTime = true;

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await StopRecordingAsync();
                        await Toast.Make("Recording stopped — maximum 1 hour reached. Please start a new record.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
                    });
                }
            };
            _timer.Start();
        }

        public void StopDurationTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= null;
                _timer = null;
            }
        }

        private async Task StartSpeechRecognitionAsync()
        {
            try
            {
                if (_speechRecognizer == null)
                {
                    _speechRecognizer = new SpeechRecognizer(speechConfig, autoLangConfig, AudioConfig.FromDefaultMicrophoneInput());

                    _speechRecognizer.Recognizing += (s, e) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _partialText = e.Result.Text?.Trim() ?? string.Empty;
                            NoteScript = $"{_transcriptBuilder}{(_partialText.Length > 0 ? " " + _partialText : string.Empty)}";
                        });
                    };

                    _speechRecognizer.Recognized += (s, e) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
                            {
                                _transcriptBuilder.AppendLine(e.Result.Text.Trim());
                                _partialText = string.Empty;
                                NoteScript = _transcriptBuilder.ToString();
                            }
                        });
                    };

                    _speechRecognizer.Canceled += async (s, e) =>
                    {
                        //if (e.Reason == CancellationReason.Error)
                        //{
                        //    string details = e.ErrorDetails ?? "Unknown error";
                        //    Console.WriteLine($"[Recognizer] Canceled: {details}");

                        //    if (details.Contains("Quota exceeded", StringComparison.OrdinalIgnoreCase))
                        //    {
                        //        // ✅ Log silently for developers
                        //        Console.WriteLine("[Speech Service] Quota exceeded. Please check Azure billing/usage.");

                        //        // Optionally, you can show a generic friendly message instead:
                        //        await MainThread.InvokeOnMainThreadAsync(() =>
                        //        {
                        //            Toast.Make("Speech service temporarily unavailable. Please try again later.",
                        //                CommunityToolkit.Maui.Core.ToastDuration.Short, 15).Show();
                        //        });

                        //        return;
                        //    }

                        //    // Otherwise, auto-restart recognition after brief delay
                        //    await _speechRecognizer.StopContinuousRecognitionAsync();
                        //    await Task.Delay(2000);
                        //    await _speechRecognizer.StartContinuousRecognitionAsync();
                        //}
                    };

                    _speechRecognizer.SessionStopped += async (s, e) => { };
                }

                if (_isRecognizerRunning)
                {
                    await _speechRecognizer.StopContinuousRecognitionAsync();
                    _isRecognizerRunning = false;
                    await Task.Delay(300);
                }

                await _speechRecognizer.StartContinuousRecognitionAsync();
                _isRecognizerRunning = true;

                //// Optional: monitor session to auto-reset every ~20 minutes
                //_ = MonitorRecognizerAsync();
            }
            catch (Exception ex)
            {
                await App.Current!.MainPage!.DisplayAlert("Speech Error",
                    "Could not start recognition: " + ex.Message, "OK");
            }
        }

        private async Task StartTranscriptionAsync()
        {
            try
            {
                if (_conversationTranscriber == null)
                {
                    _conversationTranscriber = new ConversationTranscriber(speechConfig, AudioConfig.FromDefaultMicrophoneInput());

                    _conversationTranscriber.Transcribing += (s, e) =>
                    {
                        e.Result.Duration.ToString();
                        var speaker = (string.IsNullOrEmpty(e.Result.SpeakerId) || e.Result.SpeakerId == "Unknown")
                            ? AppResources.lblScript : e.Result.SpeakerId;

                        var partial = e.Result.Text?.Trim();
                        if (string.IsNullOrEmpty(partial)) return;

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (!_liveMessages.ContainsKey(speaker))
                            {
                                var msg = new MeetingMessage
                                {
                                    Speaker = speaker,
                                    Text = partial + "...",
                                    TextColor = _speakerColors.TryGetValue(speaker, out var color)
                                        ? color : Colors.Red,
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

                    _conversationTranscriber.Transcribed += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
                        {
                            var speaker = (string.IsNullOrEmpty(e.Result.SpeakerId) || e.Result.SpeakerId == "Unknown")
                                ? AppResources.lblScript : e.Result.SpeakerId.Replace("Guest", "Speaker");

                            var text = e.Result.Text.Trim();

                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                if (!IsTextValidLanguage(text))
                                {
                                    var toast = Toast.Make(AppResources.msgUnsupportLang,
                                        CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                                    await toast.Show();
                                }

                                if (_liveMessages.ContainsKey(speaker))
                                {
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

                    _conversationTranscriber.Canceled += async (s, e) =>
                    {
                        ////MainThread.BeginInvokeOnMainThread(async () =>
                        ////{
                        ////    await App.Current!.MainPage!.DisplayAlert("Error",
                        ////        $"Recognition failed. Reason: {e.Reason}\nDetails: {e.ErrorDetails}", "OK");
                        ////});

                        //if (e.Reason == CancellationReason.Error)
                        //{
                        //    if (e.ErrorDetails.Contains("Quota exceeded", StringComparison.OrdinalIgnoreCase))
                        //    {
                        //        Console.WriteLine("[Transcriber] Quota limit hit — stopping gracefully.");
                        //        await _conversationTranscriber.StopTranscribingAsync();
                        //        await MainThread.InvokeOnMainThreadAsync(() =>
                        //        {
                        //            Toast.Make("Speech service temporarily unavailable. Please try again later.",
                        //                CommunityToolkit.Maui.Core.ToastDuration.Long, 15).Show();
                        //        });
                        //        return;
                        //    }

                        //    // ✅ For other transient network or buffer errors
                        //    if (_isTranscribing)
                        //    {
                        //        Console.WriteLine($"[Transcriber] Reset due to transient error: {e.ErrorDetails}");
                        //        try
                        //        {
                        //            await _conversationTranscriber.StopTranscribingAsync();
                        //            await Task.Delay(1000);
                        //            await _conversationTranscriber.StartTranscribingAsync();
                        //            _lastRestartTime = DateTime.UtcNow;
                        //        }
                        //        catch (Exception restartEx)
                        //        {
                        //            Console.WriteLine($"[Transcriber Restart Error] {restartEx.Message}");
                        //        }
                        //    }
                        //}
                    };

                    _conversationTranscriber.SessionStopped += (s, e) => { };
                }

                if (_isTranscribing)
                {
                    await _conversationTranscriber.StopTranscribingAsync();
                    _isTranscribing = false;
                    await Task.Delay(300);
                }

                await _conversationTranscriber.StartTranscribingAsync();
                _isTranscribing = true;

                //// ✅ Reset last restart time when session begins
                //_lastRestartTime = DateTime.UtcNow;

                //// ✅ Start background monitor to restart every ~20 mins
                //_ = MonitorTranscriberAsync();
            }
            catch (Exception ex)
            {
                await App.Current!.MainPage!.DisplayAlert("Transcriber Error",
                    "Could not start transcribing: " + ex.Message, "OK");
            }
        }


        [RelayCommand]
        public async Task ToggleRecording()
        {
            if (!IsRecording)
            {
                if (_accumulatedDuration >= MaxRecordingDuration)
                {
                    await Toast.Make("Maximum recording time reached. Please create a new record.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
                    return;
                }
                else
                {
                    await StartRecordingAsync();
                }
            }
            else
            {
                await StopRecordingAsync();
            }
        }

        private async Task StartRecordingAsync()
        {
            UserDialogs.Instance.ShowLoading();

            try
            {
                if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                {
                    await App.Current!.MainPage!.DisplayAlert(AppResources.msgWarning,
                        AppResources.msgMicrophoneaccessisrequiredtorecordaudio, AppResources.msgOk);
                    return;
                }

                PrepareForRecording();

                var filePath = Path.Combine(FileSystem.AppDataDirectory,
                    $"recording_{DateTime.Now:yyyyMMddHHmmss}.wav");

#if IOS
                await StartRecordingIOSAsync(filePath);

#elif ANDROID
                await AndroidAudioRecorder.StartAsync(filePath);

                await StartRecordingAndroidAsync(filePath);
#endif

                recordedParts.Add(filePath);

                _recordStartTime = DateTime.UtcNow;

                if (string.IsNullOrEmpty(DurationDisplay))
                    DurationDisplay = "00:00:00";

                StartDurationTimer();

                await StartRecognitionOrTranscriptionAsync();
            }
            catch (Exception ex)
            {
                await App.Current!.MainPage!.DisplayAlert(AppResources.msgError,
                    $"{AppResources.msgCouldnotstartrecording} {ex.Message}", AppResources.msgOk);
            }
            finally
            {
                UserDialogs.Instance.HideHud();
            }
        }

        private async Task StopRecordingAsync()
        {
            UserDialogs.Instance.ShowLoading();

            try
            {
                IsRecording = false;

#if IOS
                if (recorder != null)
                {
                    await recorder.Stop();
                    recorder = null;
                }
                // ✅ Deactivate the audio session to release the microphone
                var audioSession = AVFoundation.AVAudioSession.SharedInstance();
                audioSession.SetActive(false);
                audioSession.SetCategory(AVFoundation.AVAudioSessionCategory.Ambient);
#elif ANDROID

                await AndroidAudioRecorder.StopAsync();
#endif

                SaveElapsedTime();
                StopDurationTimer();

                await StopRecognitionOrTranscriptionAsync();
            }
            catch (Exception ex)
            {
                await App.Current!.MainPage!.DisplayAlert(AppResources.msgError,
                    $"{AppResources.msgCouldnotstoprecording} {ex.Message}", AppResources.msgOk);
            }
            finally
            {
#if ANDROID
                StopForegroundRecordingService();
#endif
                UserDialogs.Instance.HideHud();
            }
        }

        private async void PrepareForRecording()
        {
            IsRecording = true;
            IsShowStopBtn = true;
            IsEnable = false;

            if (SelectedLanguage == "English")
                speechConfig.SpeechRecognitionLanguage = "en-US";
            else if (SelectedLanguage == "العربية")
                speechConfig.SpeechRecognitionLanguage = "ar-EG";
        }

        private void SaveElapsedTime()
        {
            if (_recordStartTime != null)
            {
                _accumulatedDuration += DateTime.UtcNow - _recordStartTime.Value;
                _recordStartTime = null;
            }
        }

        private async Task StartRecognitionOrTranscriptionAsync()
        {
            if (SelectedScriptType.Id == 1)
                await StartSpeechRecognitionAsync();
            else if (SelectedScriptType.Id == 2)
                await StartTranscriptionAsync();
        }

        private async Task StopRecognitionOrTranscriptionAsync()
        {
            if (SelectedScriptType.Id == 1 && _speechRecognizer != null && _isRecognizerRunning)
            {
                await _speechRecognizer.StopContinuousRecognitionAsync();
                _isRecognizerRunning = false;
            }
            else if (SelectedScriptType.Id == 2 && _conversationTranscriber != null && _isTranscribing)
            {
                await _conversationTranscriber.StopTranscribingAsync();
                _isTranscribing = false;
            }
        }

        private static async Task<List<string>> SplitLargeFileAsync(string filePath, long maxPartSizeBytes = 25 * 1024 * 1024)
        {
            var parts = new List<string>();
            var buffer = new byte[81920]; // 80 KB buffer

            using var input = File.OpenRead(filePath);
            int partIndex = 0;

            while (input.Position < input.Length)
            {
                var partPath = Path.Combine(
                    Path.GetDirectoryName(filePath)!,
                    $"{Path.GetFileNameWithoutExtension(filePath)}_part{partIndex:D3}{Path.GetExtension(filePath)}"
                );

                using var output = File.Create(partPath);
                long bytesWritten = 0;
                int bytesRead;

                while (bytesWritten < maxPartSizeBytes &&
                       (bytesRead = await input.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, maxPartSizeBytes - bytesWritten)))) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, bytesRead));
                    bytesWritten += bytesRead;
                }

                parts.Add(partPath);
                partIndex++;
            }

            return parts;
        }


#if IOS
        private async Task StartRecordingIOSAsync(string filePath)
        {
            // ✅ Configure and activate the audio session
            var audioSession = AVFoundation.AVAudioSession.SharedInstance();

            audioSession.SetCategory(
                AVFoundation.AVAudioSessionCategory.PlayAndRecord,
                AVFoundation.AVAudioSessionCategoryOptions.DefaultToSpeaker |
                AVFoundation.AVAudioSessionCategoryOptions.AllowBluetooth |
                AVFoundation.AVAudioSessionCategoryOptions.AllowBluetoothA2DP
            );

            audioSession.SetMode(AVFoundation.AVAudioSession.ModeDefault, out _);
            audioSession.SetActive(true);

            // ✅ Create recorder instance
            recorder = new Cardrly.Services.NativeAudioRecorder.iOSAudioRecorder();


            // later, when cleaning up (before recorder = null)
            recorder.OnInterruptionBegan -= HandleInterruptionBegan;
            recorder.OnInterruptionEnded -= HandleInterruptionEnded;

            // subscribe
            recorder.OnInterruptionBegan += HandleInterruptionBegan;
            recorder.OnInterruptionEnded += HandleInterruptionEnded;

            await recorder.Start(filePath);
        }

        private async void HandleInterruptionBegan()
        {
            try
            {
                if (recorder?.IsRecording == true)
                    await recorder.Stop();

                await StopRecognitionOrTranscriptionAsync();
                SaveElapsedTime();
                StopDurationTimer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnInterruptionBegan Error: {ex.Message}");
            }
        }

        private async void HandleInterruptionEnded()
        {
            try
            {
                var newFilePath = Path.Combine(FileSystem.AppDataDirectory,
                    $"resume_{DateTime.Now:yyyyMMddHHmmss}.wav");

                // re-create recorder instance or call appropriate resume logic
                recorder = new iOSAudioRecorder();
                await recorder.Start(newFilePath);
                recordedParts.Add(newFilePath);

                await StartRecognitionOrTranscriptionAsync();

                _recordStartTime = DateTime.UtcNow;
                StartDurationTimer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnInterruptionEnded Error: {ex.Message}");
            }
        }
#endif

#if ANDROID
        private async Task StartRecordingAndroidAsync(string filePath)
        {
            StartForegroundRecordingService();

            var audioManager = (Android.Media.AudioManager)Platform.AppContext.GetSystemService(Context.AudioService);

            // 🔹 Create focus listener once (if not exists)
            _focusListener ??= new Cardrly.Platforms.Android.Receivers.AudioFocusChangeListener();

            // 🔹 Release any old focus tied to this same listener
            audioManager.AbandonAudioFocus(_focusListener);

            var result = audioManager.RequestAudioFocus(_focusListener, Android.Media.Stream.Music, Android.Media.AudioFocus.Gain);


            if (result != Android.Media.AudioFocusRequest.Granted)
            {
                Console.WriteLine("⚠️ Failed to gain audio focus. Trying again later.");
                return;
            }

            _focusListener.OnAudioFocusChanged = async (focus) =>
            {
                try
                {
                    if (focus == "LOST")
                    {
                        //if (recorder != null)
                        //    await recorder.StopAsync();

                        await AndroidAudioRecorder.StopAsync();

                        await StopRecognitionOrTranscriptionAsync();
                        SaveElapsedTime();
                        StopDurationTimer();
                    }
                    else if (focus == "GAIN")
                    {
                        var newFilePath = Path.Combine(FileSystem.AppDataDirectory,
                            $"resume_{DateTime.Now:yyyyMMddHHmmss}.wav");

                        //recorder = AudioManager.Current.CreateRecorder();
                        //await recorder.StartAsync(newFilePath);
                        await AndroidAudioRecorder.StartAsync(newFilePath);
                        recordedParts.Add(newFilePath);

                        await StartRecognitionOrTranscriptionAsync();

                        _recordStartTime = DateTime.UtcNow;
                        StartDurationTimer();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Audio focus handling error: {ex.Message}");
                }
            };
        }

        private void StartForegroundRecordingService()
        {
            try
            {
                var context = Platform.AppContext;
                var intent = new Intent(context, typeof(Cardrly.Platforms.Android.ForegroundAudioService));
                context.StartForegroundService(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not start foreground service: {ex.Message}");
            }
        }

        private void StopForegroundRecordingService()
        {
            try
            {
                var context = Platform.AppContext;
                var intent = new Intent(context, typeof(Cardrly.Platforms.Android.ForegroundAudioService));

                var audioManager = (Android.Media.AudioManager)Platform.AppContext.GetSystemService(Context.AudioService);

                if (_focusListener != null)
                {
                    audioManager.AbandonAudioFocus(_focusListener);
                    _focusListener = null;
                }

                context.StopService(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not stop foreground service: {ex.Message}");
            }
        }
#endif






        private async Task MonitorTranscriberAsync()
        {
            while (_isTranscribing)
            {
                try
                {
                    // Restart every 20 minutes to avoid buffer overflow
                    if ((DateTime.UtcNow - _lastRestartTime).TotalMinutes > 20)
                    {
                        Console.WriteLine("[Transcriber] Restarting to clear buffer...");

                        await _conversationTranscriber.StopTranscribingAsync();
                        await Task.Delay(1000);
                        await _conversationTranscriber.StartTranscribingAsync();

                        _lastRestartTime = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MonitorTranscriberAsync] {ex.Message}");
                }

                await Task.Delay(10000); // Check every 10 seconds
            }
        }

        private async Task MonitorRecognizerAsync()
        {
            try
            {
                while (_isRecognizerRunning)
                {
                    await Task.Delay(TimeSpan.FromMinutes(20));

                    if (_speechRecognizer != null && _isRecognizerRunning)
                    {
                        Console.WriteLine("[Recognizer] Auto-reset after 20 minutes.");
                        await _speechRecognizer.StopContinuousRecognitionAsync();
                        await Task.Delay(1000);
                        await _speechRecognizer.StartContinuousRecognitionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Recognizer Monitor] Error: {ex.Message}");
            }
        }

        //public static async Task<string> CompressToMp3CrossPlatformAsync(string wavPath)
        //{
        //    await FfmpegInitializer.EnsureInitializedAsync();// ✅ initialize only when needed

        //    var mp3Path = Path.ChangeExtension(wavPath, ".mp3");

        //    await FFMpegArguments
        //        .FromFileInput(wavPath)
        //        .OutputToFile(mp3Path, true, options => options
        //            .WithAudioCodec(AudioCodec.LibMp3Lame)
        //            .WithAudioBitrate(64))
        //        .ProcessAsynchronously();

        //    return mp3Path;
        //}

    }
}

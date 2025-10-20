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
using System.Collections.ObjectModel;
using Plugin.Maui.Audio;
using System.Text;

#if IOS
using Cardrly.Services.NativeAudioRecorder;
using Cardrly.Services.AudioRecord;
#elif ANDROID
using Android.Content;
#endif

namespace Cardrly.ViewModels
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
        string durationDisplay;

        CancellationTokenSource? _timerCts;

        [ObservableProperty]
        string noteScript;

        [ObservableProperty]
        bool isShowExpander = false;

        [ObservableProperty]
        bool isShowGetScript = false;

        [ObservableProperty]
        MeetingAiActionInfoResponse meetingInfoModel;

        private StringBuilder _transcriptBuilder = new();

#if IOS
        private INativeAudioRecorder? recorder;
#else
        private IAudioRecorder? recorder;
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

        string speechKey;
        string speechRegion;

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
                    //await ResetUi();
                    await App.Current!.MainPage!.Navigation.PopAsync();
                }
            }
            else
            {
                // Reset everything
                //await ResetUi();
                await App.Current!.MainPage!.Navigation.PopAsync();
            }
        }

        public async Task ResetUi()
        {
#if IOS
            if (recorder != null)
            {
                await recorder.Stop();
                await Task.Delay(300); // ✅ Let iOS finalize the file
                recorder = null;
            }
#else
            if (recorder != null)
            {
                await recorder.StopAsync();
                recorder = null;
            }
            // 🔹 Stop background recording service
#if ANDROID
                StopForegroundRecordingService();
                Cardrly.Platforms.Android.Receivers.PhoneCallReceiver.OnCallStateChanged = null;
#endif
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

            StopDurationTimer();
            IsRecording = false;

            if (string.IsNullOrEmpty(DurationDisplay))
                DurationDisplay = "00:00:00";

            if (_recordStartTime != null)
            {
                _accumulatedDuration = TimeSpan.Zero;
            }
            Messages.Clear();
            NoteScript = string.Empty;

            await App.Current!.MainPage!.Navigation.PopAsync();
        }

        [RelayCommand]
        public async Task StopRecording()
        {
            try
            {
#if IOS
                if (recorder != null)
                {
                    await recorder.Stop();
                    recorder = null;
                }
#else
                if (recorder != null)
                {
                    await recorder.StopAsync();
                    recorder = null;
                }
                // 🔹 Stop background recording service
#if ANDROID
                    StopForegroundRecordingService();
                    Cardrly.Platforms.Android.Receivers.PhoneCallReceiver.OnCallStateChanged = null;
#endif
#endif

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
#if ANDROID
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
#elif IOS
                    await MergeWavFilesFixedAsync(mergedFilePath, recordedParts);
#endif

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

            foreach (var path in inputPaths)
            {
                if (!File.Exists(path))
                    continue;

                var bytes = await File.ReadAllBytesAsync(path);
                if (bytes.Length <= 44)
                    continue;

                await output.WriteAsync(bytes, 44, bytes.Length - 44);
                totalDataLength += (bytes.Length - 44);
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
            await output.WriteAsync(headerStream.ToArray());

            await output.FlushAsync();
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
        public async Task ToggleRecording()
        {
            if (!IsRecording)
            {
                UserDialogs.Instance.ShowLoading();
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

                var filePath = Path.Combine(FileSystem.AppDataDirectory, $"recording_{DateTime.Now:yyyyMMddHHmmss}.wav");

                try
                {

                    if (SelectedLanguage == "English")
                        speechConfig.SpeechRecognitionLanguage = "en-US";
                    else if (SelectedLanguage == "العربية")
                        speechConfig.SpeechRecognitionLanguage = "ar-EG";

#if IOS
                    recorder = new iOSAudioRecorder();
                    await recorder.Start(filePath);
#else
                    recorder = AudioManager.Current.CreateRecorder();
                    await recorder.StartAsync(filePath);
                    // 🔹 Keep recording active when screen locks (Android)
#if ANDROID
                    StartForegroundRecordingService();

                    Cardrly.Platforms.Android.Receivers.PhoneCallReceiver.OnCallStateChanged = async (inCall) =>
                    {
                        if (inCall)
                        {
                            try
                            {
                                if (recorder != null)
                                {
                                    await recorder.StopAsync();
                                    Console.WriteLine("📞 Incoming call detected — paused recording");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Pause on call exception: {ex.Message}");
                            }
                        }
                        else
                        {
                            try
                            {
                                // Resume recording after call ends
                                var newFilePath = Path.Combine(FileSystem.AppDataDirectory, $"resume_{DateTime.Now:yyyyMMddHHmmss}.wav");
                                recorder = AudioManager.Current.CreateRecorder();
                                await recorder.StartAsync(newFilePath);
                                recordedParts.Add(newFilePath);
                                Console.WriteLine("📞 Call ended — resumed recording");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Resume after call exception: {ex.Message}");
                            }
                        }
                    };
#endif
#endif

                    // Save path for merging later
                    recordedParts.Add(filePath);

                    // Mark start point (don’t reset _accumulatedDuration, so timer continues)
                    _recordStartTime = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(DurationDisplay))
                        DurationDisplay = "00:00:00";

                    StartDurationTimer();

                    // 🎤 Handle speech recognition type
                    if (SelectedScriptType.Id == 1) // SIMPLE SCRIPT
                    {
                        await StartSpeechRecognitionAsync();
                    }
                    else if (SelectedScriptType.Id == 2) // MEETING SCRIPT
                    {
                        await StartTranscriptionAsync();
                    }

                }
                catch (Exception ex)
                {
                    await App.Current!.MainPage!.DisplayAlert(AppResources.msgError, $"{AppResources.msgCouldnotstartrecording} {ex.Message}", AppResources.msgOk);
                }
                UserDialogs.Instance.HideHud();
            }
            else
            {
                UserDialogs.Instance.ShowLoading();
                // Pause recording
                try
                {
                    // ⏸️ PAUSE RECORDING
                    IsRecording = false;

#if IOS
                    if (recorder != null)
                    {
                        await recorder.Stop();
                        recorder = null;
                    }
#else
                    if (recorder != null)
                    {
                        await recorder.StopAsync();
                        recorder = null;
                    }
                    // 🔹 Stop background recording service
#if ANDROID
                    StopForegroundRecordingService();
#endif
#endif

                    if (_recordStartTime != null)
                    {
                        // Save elapsed time into accumulator
                        _accumulatedDuration += DateTime.UtcNow - _recordStartTime.Value;
                        _recordStartTime = null;
                    }

                    StopDurationTimer();


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
                catch (Exception ex)
                {
                    await App.Current!.MainPage!.DisplayAlert(AppResources.msgError, $"{AppResources.msgCouldnotstoprecording} {ex.Message}", AppResources.msgOk);
                }
                UserDialogs.Instance.HideHud();
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

                    _speechRecognizer.Canceled += (s, e) => { };
                    _speechRecognizer.SessionStopped += (s, e) => { };
                }

                if (_isRecognizerRunning)
                {
                    await _speechRecognizer.StopContinuousRecognitionAsync();
                    _isRecognizerRunning = false;
                    await Task.Delay(300);
                }

                await _speechRecognizer.StartContinuousRecognitionAsync();
                _isRecognizerRunning = true;
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
                                        ? color : Colors.Red
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
                                ? AppResources.lblScript : e.Result.SpeakerId;

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

                    _conversationTranscriber.Canceled += (s, e) =>
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await App.Current!.MainPage!.DisplayAlert("Error",
                                $"Recognition failed. Reason: {e.Reason}\nDetails: {e.ErrorDetails}", "OK");
                        });
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
            }
            catch (Exception ex)
            {
                await App.Current!.MainPage!.DisplayAlert("Transcriber Error",
                    "Could not start transcribing: " + ex.Message, "OK");
            }
        }


#if ANDROID
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
                context.StopService(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not stop foreground service: {ex.Message}");
            }
        }
#endif
    }
}

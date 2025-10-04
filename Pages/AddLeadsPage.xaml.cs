using Azure;
using Azure.AI.TextAnalytics;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels.Leads;
using Controls.UserDialogs.Maui;
using Microsoft.CognitiveServices.Speech;
using Plugin.Maui.Audio;
using System.Text;

namespace Cardrly.Pages;

public partial class AddLeadsPage : Controls.CustomControl
{
    AddLeadViewModel Model;
    readonly IAudioRecorder _audioRecorder;
    readonly TextAnalyticsClient _textAnalyticsClient;
    readonly SpeechRecognizer _speechRecognizer;
    public AddLeadsPage(AddLeadViewModel model)
    {
        InitializeComponent();
        Model = model;
        this.BindingContext = model;
    }

    private async void OnStartRecordingClicked(object sender, EventArgs e)
    {
        try
        {
            // Disable button and show a loading indicator
            RecordButton.IsEnabled = false;


            // Step 1: Record audio
            var audioFilePath = await RecordAudioAsync();

            // Step 2: Convert speech to text
            string recognizedText = await ConvertSpeechToTextAsync();

            // Step 3: Analyze text using Azure Text Analytics
            string extractedInfo = AnalyzeText(recognizedText);

        }
        catch (Exception ex)
        {
            await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgAnerroroccurred} {ex.Message}", $"{AppResources.msgOk}");
        }
        finally
        {
            // Re-enable button and hide the loading indicator
            RecordButton.IsEnabled = true;

        }
    }

    private async Task<string> RecordAudioAsync()
    {
        if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
        {
            await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgMicrophoneaccessisrequiredtorecordaudio}", $"{AppResources.msgOk}");
            return null;
        }

        var filePath = Path.Combine(FileSystem.AppDataDirectory, "recorded_audio.wav");

        if (!_audioRecorder.IsRecording)
        {
            await _audioRecorder.StartAsync(filePath);
            await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgRecordingstarted}", $"{AppResources.msgOk}");
            RecordButton.Text = $"{AppResources.msgEndRecording}";
            return null; // Recording is ongoing
        }
        else
        {
            var recordedAudio = await _audioRecorder.StopAsync();
            if (File.Exists(filePath))
            {
                RecordButton.Text = $"{AppResources.lblStarRecording}";

                var player = AudioManager.Current.CreatePlayer(recordedAudio.GetAudioStream());
                player.Play();
                return filePath;
            }
            else
            {
                await DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgAudiofilenotsavedPleasetryagain}", $"{AppResources.msgOk}");
                RecordButton.Text = $"{AppResources.lblStarRecording}";
                return null;
            }

        }
    }

    private async Task<string> ConvertSpeechToTextAsync()
    {
        try
        {
            // Start speech recognition
            var result = await _speechRecognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                return result.Text; // Return the recognized speech
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                return $"{AppResources.msgSpeechnotrecognizedPleasetryagain}";
            }
            else
            {
                return $"{AppResources.msgSpeechrecognitionfailed} {result.Reason}";
            }
        }
        catch (Exception ex)
        {
            return $"{AppResources.msgErrorduringspeechrecognition} {ex.Message}";
        }
    }

    private string AnalyzeText(string text)
    {
        try
        {
            var entities = _textAnalyticsClient.RecognizeEntities(text);
            var result = new StringBuilder();

            foreach (var entity in entities.Value)
            {
                if (entity.Category.ToString().Contains("user"))
                {
                    Model.Request!.FullName = entity.Text;
                }
                if (entity.Category.ToString().Contains("phone"))
                {
                    Model.Request!.Phone = entity.Text;
                }
                if (entity.Category.ToString().ToLower().Contains("email"))
                {
                    Model.Request!.Email = entity.Text;
                }
                if (entity.Category.ToString().ToLower().Contains("address"))
                {
                    Model.Request!.Address = entity.Text;
                }
                if (entity.Category.ToString().ToLower().Contains("company"))
                {
                    Model.Request!.Company = entity.Text;
                }
                result.AppendLine($"Entity: {entity.Text}, Type: {entity.Category}");
            }
            string res = result.ToString();
            return res;
        }
        catch (Exception ex)
        {
            return $"Error analyzing text: {ex.Message}";
        }
    }
}
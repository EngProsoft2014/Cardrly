using Azure;
using Azure.AI.TextAnalytics;
using Cardrly.ViewModels.Leads;
using Controls.UserDialogs.Maui;
using Microsoft.CognitiveServices.Speech;
using Plugin.Maui.Audio;
using System.Text;

namespace Cardrly.Pages;

public partial class AddLeadsPage : Controls.CustomControl
{
    AddLeadViewModel Model;
    readonly IAudioManager _audioManager;
    readonly IAudioRecorder _audioRecorder;
    readonly TextAnalyticsClient _textAnalyticsClient;
    readonly SpeechRecognizer _speechRecognizer;
    public AddLeadsPage(AddLeadViewModel model,IAudioManager audioManager)
	{
		InitializeComponent();
        Model = model;
        this.BindingContext = model;
        // Initialize audio manager
        _audioManager = audioManager;
        _audioRecorder = audioManager.CreateRecorder();

        // Initialize Azure Text Analytics client
        var endpoint = "https://fillformanalytics.cognitiveservices.azure.com/";//"YOUR_AZURE_TEXT_ANALYTICS_ENDPOINT";
        var apiKey = "8AA6fsh0vZ8E7ZBnhcWOteVthW4ONocsNKruqwsCoUJNCsRDVLsrJQQJ99BAACYeBjFXJ3w3AAAaACOGLOTY";//"YOUR_AZURE_TEXT_ANALYTICS_KEY";
        _textAnalyticsClient = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

        // Initialize Azure Speech Service client
        var speechKey = "81f8gkSQe5THESeGnd1mArbfahQv9DNPbzmf6RjOJgdF7V6PZT1XJQQJ99ALACYeBjFXJ3w3AAAYACOGP22X"; //"YOUR_AZURE_SPEECH_KEY";
        var speechRegion = "eastus"; //"YOUR_REGION"; // Example: "eastus"
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        _speechRecognizer = new SpeechRecognizer(speechConfig);
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
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
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
            await DisplayAlert("Permission Denied", "Microphone access is required to record audio.", "OK");
            return null;
        }

        var filePath = Path.Combine(FileSystem.AppDataDirectory, "recorded_audio.wav");

        if (!_audioRecorder.IsRecording)
        {
            await _audioRecorder.StartAsync(filePath);
            await DisplayAlert("Recording", "Recording started...", "OK");
            RecordButton.Text = "End Recording";
            return null; // Recording is ongoing
        }
        else
        {
            var recordedAudio = await _audioRecorder.StopAsync();
            if (File.Exists(filePath))
            {
                await DisplayAlert("Recording Stopped", $"Audio saved to: {filePath}", "OK");
                RecordButton.Text = "Start Recording";

                var player = AudioManager.Current.CreatePlayer(recordedAudio.GetAudioStream());
                player.Play();
                return filePath;
            }
            else
            {
                await DisplayAlert("Error", "Audio file not saved. Please try again.", "OK");
                RecordButton.Text = "Start Recording";
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
                return "Speech not recognized. Please try again.";
            }
            else
            {
                return $"Speech recognition failed: {result.Reason}";
            }
        }
        catch (Exception ex)
        {
            return $"Error during speech recognition: {ex.Message}";
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
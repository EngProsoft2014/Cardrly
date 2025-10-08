using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;

namespace Cardrly.Pages.MeetingsScript;

public partial class RecordPage : Controls.CustomControl
{

    NotesScriptDetailsViewModel viewModel;
    CancellationTokenSource? _cts;

    public RecordPage(NotesScriptDetailsViewModel model)
    {
        InitializeComponent();
        this.BindingContext = viewModel = model;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        viewModel.IsRecording = false;
        viewModel.IsShowStopBtn = false;
        viewModel.IsEnableLang = true;
        viewModel.IsEnableScriptType = true;
    }

    protected override bool OnBackButtonPressed()
    {
        // Run the async code on the UI thread
        Dispatcher.Dispatch(async () =>
        {
            if (viewModel.Messages.Count > 0 || !string.IsNullOrEmpty(viewModel.NoteScript))
            {
                bool Pass = await App.Current!.MainPage!.DisplayAlert(AppResources.Info, AppResources.msgDoyouwanttosavetherecording, AppResources.msgOk, AppResources.btnCancel);

                if (Pass)
                {
                    await viewModel.StopRecording();
                    await Navigation.PopAsync();
                }
                else
                {
                    // Reset everything
                    await viewModel.ResetUi();
                }
            }
            else
            {
                // Reset everything
                await viewModel.ResetUi();
            }

        });

        // Return true to prevent the default behavior
        return true;
    }

    private void picLang_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var selectedLanguage = picker.SelectedItem as string;

        viewModel.SelectedLanguage = selectedLanguage;

    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var selectedScriptType = picker.SelectedItem as ScriptTypeModel;

        viewModel.SelectedScriptType = selectedScriptType;
    }
}
using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;
using Cardrly.ViewModels.MeetingsAi;

namespace Cardrly.Pages.MeetingsScript;

public partial class RecordPage : Controls.CustomControl
{

    RecordViewModel viewModel;

    public RecordPage(RecordViewModel model)
    {
        InitializeComponent();
        this.BindingContext = viewModel = model;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is RecordViewModel vm)
        {
            vm.StopDurationTimer();
        }
    }

    protected override bool OnBackButtonPressed()
    {
        // Run the async code on the UI thread
        Dispatcher.Dispatch(async () =>
        {
            if (viewModel.Messages.Count > 0 || !string.IsNullOrEmpty(viewModel.NoteScript))
            {
                await Mopups.Services.MopupService.Instance.PushAsync(new ConfirmRecordPopup(viewModel));
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

    //private void picLang_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    var picker = sender as Picker;
    //    var selectedLanguage = picker.SelectedItem as string;

    //    viewModel.SelectedLanguage = selectedLanguage;

    //}

    //private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    var picker = sender as Picker;
    //    var selectedScriptType = picker.SelectedItem as ScriptTypeModel;

    //    viewModel.SelectedScriptType = selectedScriptType;
    //}
}
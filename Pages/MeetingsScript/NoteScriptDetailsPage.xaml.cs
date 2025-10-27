
using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.Resources.Lan;
using Cardrly.Services.AudioStream;
using Cardrly.ViewModels;
using Cardrly.ViewModels.MeetingsAi;

namespace Cardrly.Pages.MeetingsScript;


public partial class NoteScriptDetailsPage : Controls.CustomControl
{

    NotesScriptDetailsViewModel viewModel;
    CancellationTokenSource? _cts;

    public NoteScriptDetailsPage(NotesScriptDetailsViewModel model)
    {
        InitializeComponent();
        this.BindingContext = viewModel = model;
    }

    protected override bool OnBackButtonPressed()
    {
        // Run the async code on the UI thread
        Dispatcher.Dispatch(() =>
        {
            //viewModel.CheckAudio(viewModel.AudioDetails);
            viewModel._audioService.Stop();
            Navigation.PopAsync();
        });

        // Return true to prevent the default behavior
        return true;
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if(e.Value)
        {
            viewModel.IsShowGetScript = true;
        }
        else
        {
            if(viewModel.MeetingInfoModel?.MeetingAiActionRecords.Count > 0)
            {
                bool IsAnyScripting = viewModel.MeetingInfoModel.MeetingAiActionRecords.Any(x => x.IsScript);
                if (IsAnyScripting)
                {
                    viewModel.IsShowGetScript = true;
                }
                else
                {
                    viewModel.IsShowGetScript = false;
                }
            }
        }
    }
}
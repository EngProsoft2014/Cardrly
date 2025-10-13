using Cardrly.Models.MeetingAiActionRecordAnalyze;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;

namespace Cardrly.Pages.MeetingsScript;

public partial class FullScreenScriptPage : Controls.CustomControl
{
	public FullScreenScriptPage(NotesScriptDetailsViewModel viewModel, MeetingAiActionRecordAnalyzeResponse modelScript, int titlePage)
	{
		InitializeComponent();
        this.BindingContext = viewModel;

        if(titlePage == 1)
        {
            edtScript.Text = modelScript.AnalyzeScript;
            lblTitle.Text = AppResources.lblSummary;
        }
        else if(titlePage == 2)
        {
            edtScript.Text = modelScript.AudioAllScript;
            lblTitle.Text = AppResources.lblAllScript;
        }

    }

}
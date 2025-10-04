using Cardrly.ViewModels;

namespace Cardrly.Pages.MeetingsScript;

public partial class FullScreenScriptPage : Controls.CustomControl
{
	public FullScreenScriptPage(NotesScriptDetailsViewModel viewModel, string? script)
	{
		InitializeComponent();
        this.BindingContext = viewModel;
        edtScript.Text = script;
    }

}
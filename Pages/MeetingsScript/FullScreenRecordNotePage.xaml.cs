using Cardrly.ViewModels;

namespace Cardrly.Pages.MeetingsScript;

public partial class FullScreenRecordNotePage : Controls.CustomControl
{

    public FullScreenRecordNotePage(NotesScriptDetailsViewModel viewModel, string? note)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
		edtNote.Text = note;
        if (string.IsNullOrEmpty(note))
        {
            imgPDF.IsVisible = false;
        }
    }
}
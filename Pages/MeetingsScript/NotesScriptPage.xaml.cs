
using Azure.AI.TextAnalytics;
using Cardrly.Helpers;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;
using Microsoft.CognitiveServices.Speech;
using Plugin.Maui.Audio;

namespace Cardrly.Pages.MeetingsScript;

public partial class NotesScriptPage : Controls.CustomControl
{
	NotesScriptViewModel viewModel;
    CancellationTokenSource? _cts;

    public NotesScriptPage(NotesScriptViewModel model)
	{
		InitializeComponent();
		this.BindingContext = viewModel = model;     
    }

}